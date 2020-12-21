using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.dispose;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using com.tinylabproductions.TLPLib.Dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using com.tinylabproductions.TLPLib.Pools;
using com.tinylabproductions.TLPLib.Reactive;using pzd.lib.reactive;

using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.dispose;
using pzd.lib.functional;
using UnityEngine;
using static pzd.lib.typeclasses.Str;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  [PublicAPI] public partial class DConsole {
    public enum Direction { Left, Up, Right, Down }

    [Record] public partial struct Command {
      public readonly string cmdGroup, name;
      public readonly Option<KeyCodeWithModifiers> shortcut; 
      public readonly Action<API> run;
      public readonly Func<bool> canShow;
      /// <summary>If false this command will be removed from commands list on DConsole re-render.</summary>
      public readonly bool persistent;

      public string label => shortcut.valueOut(out var sc) ? $"[{s(sc)}] {name}" : name;
    }

    [Record]
    partial struct Instance {
      public readonly DebugConsoleBinding view;
      public readonly DynamicLayout.Init dynamicVerticalLayout;
      public readonly Application.LogCallback logCallback;
      public readonly GameObjectPool<VerticalLayoutLogEntry> pool;
      public readonly IDisposableTracker tracker;
    }

    [Record]
    public partial struct LogEntry {
      public readonly string message;
      public readonly LogType type;
    }

    static readonly Deque<LogEntry> logEntries = new Deque<LogEntry>();
    static readonly LazyVal<DConsole> _instance = F.lazy(() => new DConsole());
    public static DConsole instance => _instance.strict;
    static bool dConsoleUnlocked;
    
    [LazyProperty, Implicit] static ILog log => Log.d.withScope(nameof(DConsole));

    [RuntimeInitializeOnLoadMethod]
    static void registerLogMessages() {
      if (!Application.isEditor) {
        // In editor we have the editor console, so this is not really needed.
        Application.logMessageReceivedThreaded += (message, stacktrace, type) => {
          lock (logEntries) {
            const int MAX_COUNT = 200;
            while (!Log.d.isDebug() && !Debug.isDebugBuild && logEntries.Count > MAX_COUNT) {
              logEntries.RemoveFront();
            }
            logEntries.Add(new LogEntry(message, type));
          }
        };
      }
    }

    [Conditional("UNITY_EDITOR"), RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void cleanUpCommandsOnRestart() {
      // With quick Unity restarts the view can be gone, but the static variable still hold the reference.
      foreach (var dConsole in _instance.value) {
        dConsole.commands.Clear();
      }
    }

    DConsole() {
      var r = registrarFor(nameof(DConsole), NeverDisposeDisposableTracker.instance, persistent: true);
      r.register("Run GC", GC.Collect);
      r.register("Self-test", () => "self-test");
      r.register("Future Self-test", () => Future.delay(Duration.fromSeconds(1), () => "after 1 s", TimeContext.unscaledTime));
      
      void clearVisibleLog() {
        foreach (var i in instance.current) {
          i.dynamicVerticalLayout.clearLayoutData();
        }
      }
      r.register("Clear visible log", clearVisibleLog);
      r.register("Clear saved log", () => {
        logEntries.Clear();
        clearVisibleLog();
      });
    }

    public delegate void OnShow(DConsole console);

    readonly IDictionary<string, List<Command>> commands = new Dictionary<string, List<Command>>();
    event OnShow onShow;

    public bool isActiveAndMaximized => current.valueOut(out var instance) && !instance.view.minimised;

    public ISubscription registrarOnShow(
      IDisposableTracker tracker, string prefix, Action<DConsole, DConsoleRegistrar> action
    ) {
      if (!Application.isPlaying) {
        return Subscription.empty;
      }

      void onShowDo(DConsole console) {
        var r = console.registrarFor(prefix, tracker, persistent: false);
        action(console, r);
      }
      
      var sub = new Subscription(() => onShow -= onShowDo);
      onShow += onShowDo;
      tracker.track(sub);
      return sub;
    }

    Option<Instance> current = F.none<Instance>();

    public static readonly ImmutableList<int> DEFAULT_MOUSE_SEQUENCE = ImmutableList.Create(0, 1, 3, 2, 0, 2, 3, 1, 0);
    public static readonly ImmutableList<Direction> DEFAULT_DIRECTION_SEQUENCE =
      ImmutableList.Create(
        Direction.Left, Direction.Right,
        Direction.Left, Direction.Right,
        Direction.Left, Direction.Up,
        Direction.Right, Direction.Down,
        Direction.Right, Direction.Up
      );

    public static readonly DebugSequenceMouseData DEFAULT_MOUSE_DATA = new DebugSequenceMouseData();
    public class DebugSequenceMouseData {
      public readonly int width, height;
      public readonly ImmutableList<int> sequence;

      public DebugSequenceMouseData(int width=2, int height=2, ImmutableList<int> sequence=null) {
        this.width = width;
        this.height = height;
        this.sequence = sequence ?? DEFAULT_MOUSE_SEQUENCE;
      }
    }

    public static readonly DebugSequenceDirectionData DEFAULT_DIRECTION_DATA = new DebugSequenceDirectionData();
    public class DebugSequenceDirectionData {
      public readonly string horizonalAxisName, verticalAxisName;
      public readonly Duration timeframe;
      public readonly ImmutableList<Direction> sequence;

      public DebugSequenceDirectionData(
        string horizonalAxisName="Horizontal",
        string verticalAxisName="Vertical",
        TimeSpan timeframe=default,
        ImmutableList<Direction> sequence=null
      ) {
        this.horizonalAxisName = horizonalAxisName;
        this.verticalAxisName = verticalAxisName;
        this.timeframe = timeframe == default ? 5.seconds() : timeframe;
        sequence ??= DEFAULT_DIRECTION_SEQUENCE;
        this.sequence = sequence;

        for (var idx = 0; idx < sequence.Count - 1; idx++) {
          var current = sequence[idx];
          var next = sequence[idx + 1];
          if (current == next) throw new ArgumentException(
            $"{nameof(DebugSequenceDirectionData)} sequence can't contain subsequent elements! " +
            $"Found {current} at {idx} & {idx + 1}.",
            nameof(sequence)
          );
        }
      }
    }

    public static IRxObservable<Unit> registerDebugSequence(
      IDisposableTracker tracker, Option<string> unlockCode,
      ITimeContext timeContext=null,
      DebugSequenceMouseData mouseData=null, Option<DebugSequenceDirectionData> directionDataOpt=default,
      DebugConsoleBinding binding=null, Option<KeyCodeWithModifiers> keyboardShortcutOpt = default,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      Option.ensureValue(ref directionDataOpt);
      Option.ensureValue(ref keyboardShortcutOpt);

      mouseData ??= DEFAULT_MOUSE_DATA;
      timeContext ??= TimeContext.DEFAULT;

      var mouseObs =
        new RegionClickObservable(mouseData.width, mouseData.height)
        .sequenceWithinTimeframe(
          tracker, mouseData.sequence, 3,
          // ReSharper disable ExplicitCallerInfoArgument
          callerMemberName: callerMemberName,
          callerFilePath: callerFilePath,
          callerLineNumber: callerLineNumber
          // ReSharper restore ExplicitCallerInfoArgument
        );

      var directionObs = directionDataOpt.fold(
        Observable<Unit>.empty,
        directionData => {
          var directions = ObservableU.everyFrame.collect(_ => {
            var horizontal = Input.GetAxisRaw(directionData.horizonalAxisName);
            var vertical = Input.GetAxisRaw(directionData.verticalAxisName);
            // Both are equal, can't decide.
            if (Math.Abs(horizontal - vertical) < 0.001f) return None._;
            return
              Math.Abs(horizontal) > Math.Abs(vertical)
              ? F.some(horizontal > 0 ? Direction.Right : Direction.Left)
              : F.some(vertical > 0 ? Direction.Up : Direction.Down);
          }).changedValues();

          return
            directions
            .withinTimeframe(directionData.sequence.Count, directionData.timeframe, timeContext)
            .filter(l => l.Select(t => t.Item1).SequenceEqual(directionData.sequence))
            .discardValue();
        }
      );

      var keyboardShortcutObs = keyboardShortcutOpt.fold(
        Observable<Unit>.empty,
        kc => ObservableU.everyFrame.filter(_ => kc.getKeyDown)
      );

      var obs = mouseObs.joinAll(new [] {directionObs, keyboardShortcutObs});
      obs.subscribe(tracker, _ => instance.show(unlockCode, binding));
      return obs;
    }

    public ISubscription register(IDisposableTracker tracker, Command command) {
      foreach (var shortcut in command.shortcut) {
        if (checkShortcutForDuplication(shortcut)) {
          command = command.withShortcut(None._);
        }
      }

      var list = commands.getOrUpdate(command.cmdGroup, () => new List<Command>());
      list.Add(command);
      var sub = new Subscription(() => list.Remove(command));
      tracker.track(sub, callerMemberName: $"DConsole register: {command.cmdGroup}/{command.name}");
      return sub;

      bool checkShortcutForDuplication(KeyCodeWithModifiers shortcut) {
        var hasConflicts = false;
        foreach (var (groupName, groupCommands) in commands) {
          foreach (var otherCommand in groupCommands) {
            if (otherCommand.shortcut.exists(shortcut)) {
              Debug.LogError(
                $"{command.cmdGroup}/{command.name} shortcut {s(shortcut)} " +
                $"conflicts with {groupName}/{otherCommand.name}"
              );
              hasConflicts = true;
            }
          }
        }

        return hasConflicts;
      }
    }

    public DConsoleRegistrar registrarFor(string prefix, IDisposableTracker tracker, bool persistent) =>
      new DConsoleRegistrar(this, prefix, tracker, persistent);
    
    public void show(Option<string> unlockCode, DebugConsoleBinding binding = null) {
      var tracker = new DisposableTracker();
      binding = binding ? binding : Resources.Load<DebugConsoleBinding>("Debug Console Prefab");

      {
        if (current.valueOut(out var currentInstance)) {
          currentInstance.view.toggleMinimised();
          return;
        }
      }
      var maybeOnShow = onShow.opt();
      onShow = null;
      maybeOnShow.getOrNull()?.Invoke(this);

      BoundButtonList commandButtonList = null;
      var selectedGroup = Option<SelectedGroup>.None;
      var view = binding.clone();
      view.hideModals();
      
      var commandsList = setupList(
        None._, view.commands, clearFilterText: true,
        () => selectedGroup.fold(ImmutableList<ButtonBinding>.Empty, _ => _.commandButtons)
      );
      
      APIImpl apiForClosures = null;
      var api = apiForClosures = new APIImpl(view, rerender: rerender);
      Object.DontDestroyOnLoad(view);

      commandButtonList = setupGroups(clearCommandsFilterText: true);
      
      var logEntryPool = GameObjectPool.a(GameObjectPool.Init<VerticalLayoutLogEntry>.noReparenting(
        nameof(DConsole) + " log entry pool",
        () => view.logEntry.prefab.clone()
      ));

      var cache = new List<string>();
      var layout = new DynamicLayout.Init(
        view.dynamicLayout,
        // ReSharper disable once InconsistentlySynchronizedField
        logEntries
          .SelectMany(e => createEntries(e, logEntryPool, cache, view.lineWidth))
          .Select(_ => _.upcast(default(DynamicLayout.IElementData))),
        tracker,
        renderLatestItemsFirst: true
      );

      var logCallback = onLogMessageReceived(logEntryPool, cache);
      Application.logMessageReceivedThreaded += logCallback;
      // Make sure to clean up on app quit to prevent problems with unity quick play mode enter.
      ASync.onAppQuit.subscribe(view.gameObject.asDisposableTracker(), _ => destroy());
      view.closeButton.onClick.AddListener(destroy);
      view.minimiseButton.onClick.AddListener(view.toggleMinimised);
      view.onUpdate += () => {
        foreach (var kv in commands) {
          foreach (var command in kv.Value) {
            foreach (var shortcut in command.shortcut) {
              if (shortcut.getKeyDown) {
                command.run(api);
              }
            }
          }
        }
      };

      current = new Instance(view, layout, logCallback, logEntryPool, tracker).some();

      BoundButtonList setupGroups(bool clearCommandsFilterText) {
        var groupButtons = commands.OrderBySafe(_ => _.Key).Select(commandGroup => {
          var validGroupCommands = commandGroup.Value.Where(cmd => cmd.canShow()).ToArray();
          var button = addButton(view.buttonPrefab, view.commandGroups.holder.transform);
          Action show = null;
          // ReSharper disable once PossibleNullReferenceException, AccessToModifiedClosure
          show = showThisGroup;
          button.text.text = commandGroup.Key;
          button.button.onClick.AddListener(showThisGroup);
          return button;

          void showThisGroup() {
            // ReSharper disable once AccessToModifiedClosure
            var commandButtons = showGroup(view, apiForClosures, commandGroup.Key, validGroupCommands);
            selectedGroup = Some.a(new SelectedGroup(button, commandButtons));
          }
        }).ToImmutableList();
        var list = setupList(
          unlockCode, view.commandGroups, clearFilterText: clearCommandsFilterText, 
          () => groupButtons
        );
        return new BoundButtonList(groupButtons, list);
      }

      void rerender() {
        var maybeSelectedGroupName = selectedGroup.map(_ => _.groupButton.text.text);
        log.info($"Re-rendering DConsole, currently selected group = {maybeSelectedGroupName}.");

        // Update command lists.
        foreach (var (_, groupCommands) in commands) {
          groupCommands.removeWhere(cmd => !cmd.persistent);
        }

        foreach (var groupName in commands.Keys.ToArray()) {
          if (commands[groupName].isEmpty()) commands.Remove(groupName);
        }

        maybeOnShow.getOrNull()?.Invoke(this);

        // Clean up existing groups
        {
          // ReSharper disable once AccessToModifiedClosure
          var existingGroups = commandButtonList;
          System.Diagnostics.Debug.Assert(existingGroups != null, nameof(existingGroups) + " != null");
          existingGroups.list.subscription.Dispose();
          foreach (var existingGroup in existingGroups.buttons) {
            existingGroup.button.destroyGameObject();
          }
        }
        var groups = commandButtonList = setupGroups(clearCommandsFilterText: false);

        {
          if (
            maybeSelectedGroupName.valueOut(out var selectedGroupName)
            && groups.buttons.findOut(selectedGroupName, (g, n) => g.text.text == n, out var group)
          ) {
            group.button.onClick.Invoke();
            commandsList.applyFilter();
          }
        }
      }
    }
    
    // DO NOT generate comparer and hashcode - we need reference equality for dynamic vertical layout!
    [Record(GenerateComparer = false, GenerateGetHashCode = false)]
    partial class DynamicVerticalLayoutLogElementData : DynamicLayout.IElementWithViewData {
      readonly GameObjectPool<VerticalLayoutLogEntry> pool;
      readonly VerticalLayoutLogEntry.Data data;
      
      public float sizeInScrollableAxis => 20;
      public Percentage sizeInSecondaryAxis => new Percentage(1f);
      public Option<DynamicLayout.IElementWithViewData> asElementWithView => 
        this.some<DynamicLayout.IElementWithViewData>();

      public DynamicLayout.IElementView createItem(Transform parent) {
        var logEntry = pool.BorrowDisposable();
        logEntry.value.transform.SetParent(parent, false);
        return new VerticalLayoutLogEntry.Init(logEntry, data);      
      }
    }

    static SetUpList setupList(
      Option<string> unlockCodeOpt, DebugConsoleListBinding listBinding, bool clearFilterText,
      Func<IEnumerable<ButtonBinding>> contents
    ) {
      listBinding.clearFilterButton.onClick.AddListener(onClearFilter);
      listBinding.filterInput.onValueChanged.AddListener(update);
      if (clearFilterText) listBinding.filterInput.text = "";
      applyFilter();

      var sub = new Subscription(() => {
        listBinding.clearFilterButton.onClick.RemoveListener(onClearFilter);
        listBinding.filterInput.onValueChanged.RemoveListener(update);
      });
      return new SetUpList(applyFilter, sub);
      
      void onClearFilter() => listBinding.filterInput.text = "";
      void applyFilter() => update(listBinding.filterInput.text);

      void update(string query) {
        if (unlockCodeOpt.valueOut(out var unlockCode)) {
          if (unlockCode.Equals(query, StringComparison.OrdinalIgnoreCase)) {
            dConsoleUnlocked = true;
            // disable filter while query matches unlock code
            query = "";
          }
        }
        var hideButtons = unlockCodeOpt.isSome && !dConsoleUnlocked;
        var showButtons = !hideButtons;
        foreach (var button in contents()) {
          var active = showButtons && button.text.text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
          button.gameObject.SetActive(active);
        }
      }
    }

    static ImmutableList<ButtonBinding> showGroup(
      DebugConsoleBinding view, API api, string groupName, IEnumerable<Command> commands
    ) {
      view.commandGroupLabel.text = groupName;
      var commandsHolder = view.commands.holder;
      foreach (var t in commandsHolder.transform.children()) Object.Destroy(t.gameObject);
      return commands.Select(command => {
        var button = addButton(view.buttonPrefab, commandsHolder.transform);
        button.text.text = command.label;
        button.button.onClick.AddListener(() => command.run(api));
        return button;
      }).ToImmutableList();
    }

    static ButtonBinding addButton(ButtonBinding prefab, Transform target) {
      var button = prefab.clone();
      // Parent of RectTransform is being set with parent property.
      // Consider using the SetParent method instead, with the worldPositionStays
      // argument set to false. This will retain local orientation and scale rather
      // than world orientation and scale, which can prevent common UI scaling issues.
      button.GetComponent<RectTransform>().SetParent(target, worldPositionStays: false);
      return button;
    }

    static IEnumerable<DynamicVerticalLayoutLogElementData> createEntries(
      LogEntry data, GameObjectPool<VerticalLayoutLogEntry> pool,
      List<string> cache, float lineWidth
    ) {
      string typeToString(LogType t) {
        switch (t) {
          case LogType.Error: return " ERROR";
          case LogType.Assert: return " ASSERT";
          case LogType.Warning: return " WARN";
          case LogType.Log: return "";
          case LogType.Exception: return " EXCEPTION";
          default: throw new Exception(t.ToString());
        }
      }

      Color typeToColor(LogType t) {
        switch (t) {
          case LogType.Error:
          case LogType.Exception:
            return Color.red;
          case LogType.Assert: return Color.magenta;
          case LogType.Warning: return new Color32(213, 144, 0, 255);
          case LogType.Log: return Color.black;
          default: throw new Exception();
        }
      }

      var shortText = $"{DateTime.Now:hh:mm:ss}{typeToString(data.type)} {data.message}";

      // letter width can't be smaller, tested on galaxy S5
      const float LETTER_WIDTH = 11.3f;
      var charCount = Mathf.RoundToInt(lineWidth / LETTER_WIDTH);

      var color = typeToColor(data.type);
      shortText.distributeText(charCount, cache);
      for (var idx = cache.Count - 1; idx >= 0; idx--) {
        var e = cache[idx];
        yield return new DynamicVerticalLayoutLogElementData(pool, new VerticalLayoutLogEntry.Data(e, color));
      }
    }
    
    Application.LogCallback onLogMessageReceived(
      GameObjectPool<VerticalLayoutLogEntry> pool,
      List<string> resultsTo
    ) =>
      (message, stackTrace, type) => {
        if (!current.isSome) return;
        ASync.OnMainThread(() => {
          // The instance can go away while we're switching threads.
          foreach (var instance in current) {
            foreach (var e in createEntries(
              new LogEntry(message, type), pool, resultsTo,
              instance.view.lineWidth
            )) instance.dynamicVerticalLayout.appendDataIntoLayoutData(e);
          }
        });
      };

    public void destroy() {
      foreach (var instance in current) {
        Debug.Log("Destroying DConsole.");
        Application.logMessageReceivedThreaded -= instance.logCallback;
        instance.tracker.Dispose();
        instance.pool.dispose(Object.Destroy);
        Object.Destroy(instance.view.gameObject);
      }
      current = None._;
    }
  }

  public delegate Option<Obj> HasObjFunc<Obj>();

  [PublicAPI] public interface ModalInputAPI {
    public string inputText { get; set; }
    public string errorText { get; set; }
    public void closeDialog();
  }

  class ModalInputAPIImpl : ModalInputAPI {
    readonly DebugConsoleBinding view;

    public ModalInputAPIImpl(DebugConsoleBinding view) => this.view = view;

    public string inputText { get => view.inputModal.input.text; set => view.inputModal.input.text = value; }
    public string errorText { get => view.inputModal.error.text; set => view.inputModal.error.text = value; }
    public void closeDialog() => view.hideModals();
  }
  
  [PublicAPI] public interface API {
    void showModalInput(
      string inputLabel, string inputPlaceholder,
      ButtonData<ModalInputAPI> button1, Option<ButtonData<ModalInputAPI>> button2 = default
    );

    void rerender();
  }

  class APIImpl : API {
    readonly DebugConsoleBinding view;
    readonly Action _rerender;

    public APIImpl(DebugConsoleBinding view, Action rerender) {
      this.view = view;
      _rerender = rerender;
    }

    public void showModalInput(
      string inputLabel, string inputPlaceholder, 
      ButtonData<ModalInputAPI> button1, Option<ButtonData<ModalInputAPI>> button2 = default
    ) {
      view.showModal(inputModal: true);
      var m = view.inputModal;
      m.label.text = inputLabel;
      m.error.text = "";
      m.inputPlaceholder.text = inputPlaceholder;
      var inputApi = new ModalInputAPIImpl(view);
      setupButton(m.button1, button1);
      m.button2.button.setActiveGO(button2.isSome);
      { if (button2.valueOut(out var b2)) setupButton(m.button2, b2); }

      void setupButton(ButtonBinding b, ButtonData<ModalInputAPI> data) {
        b.text.text = data.label;
        b.button.onClick.RemoveAllListeners();
        b.button.onClick.AddListener(() => data.onClick(inputApi));
      }
    }

    public void rerender() => _rerender();
  }
    
  [Record(GenerateConstructor = ConstructorFlags.Apply)] public sealed partial class ButtonData<A> {
    public readonly string label;
    public readonly Action<A> onClick;
  }

  [PublicAPI] public static partial class ButtonData {
    public static readonly ButtonData<ModalInputAPI> cancel = a<ModalInputAPI>("Cancel", api => api.closeDialog());
  }

  /// <summary>Set-up button list instance.</summary>
  [Record] sealed partial class SetUpList {
    public readonly Action applyFilter;
    public readonly ISubscription subscription;
  }
  
  /// <summary>List of all the buttons and it's list control instance.</summary>
  [Record] sealed partial class BoundButtonList {
    public readonly ImmutableList<ButtonBinding> buttons;
    public readonly SetUpList list;
  }

  [Record] sealed partial class SelectedGroup {
    public readonly ButtonBinding groupButton;
    public readonly ImmutableList<ButtonBinding> commandButtons;
  }
}
