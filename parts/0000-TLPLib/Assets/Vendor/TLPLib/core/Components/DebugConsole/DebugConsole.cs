using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.ui;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Pools;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.exts;
using UnityEngine;
using Object = UnityEngine.Object;
using Option = com.tinylabproductions.TLPLib.Functional.Option;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  [PublicAPI] public partial class DConsole {
    public enum Direction { Left, Up, Right, Down }

    public struct Command {
      public readonly string cmdGroup, name;
      public readonly Action run;

      public Command(string cmdGroup, string name, Action run) {
        this.cmdGroup = cmdGroup;
        this.name = name;
        this.run = run;
      }
    }

    [Record]
    partial struct Instance {
      public readonly DebugConsoleBinding view;
      public readonly DynamicVerticalLayout.Init dynamicVerticalLayout;
      public readonly Application.LogCallback logCallback;
      public readonly GameObjectPool<VerticalLayoutLogEntry> pool;
    }

    [Record]
    public partial struct LogEntry {
      public readonly string message;
      public readonly LogType type;
    }

    static readonly Deque<LogEntry> logEntries = new Deque<LogEntry>();
    static LazyVal<DConsole> _instance = F.lazy(() => new DConsole());
    public static DConsole instance => _instance.strict;

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

    DConsole() {
      var r = registrarFor(nameof(DConsole), NeverDisposeDisposableTracker.instance);
      r.register("Run GC", GC.Collect);
      r.register("Self-test", () => "self-test");
      r.register("Future Self-test", () => Future.delay(Duration.fromSeconds(1), () => "after 1 s"));
      
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

    public ISubscription registrarOnShow(
      IDisposableTracker tracker, string prefix, Action<DConsole, DConsoleRegistrar> action
    ) {
      if (!Application.isPlaying) {
        return Subscription.empty;
      }

      void onShowDo(DConsole console) {
        var r = console.registrarFor(prefix, tracker);
        action(console, r);
      }
      
      var sub = new Subscription(() => onShow -= onShowDo);
      onShow += onShowDo;
      tracker.track(sub);
      return sub;
    }

    Functional.Option<Instance> current = F.none<Instance>();

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
        Duration timeframe=default(Duration),
        ImmutableList<Direction> sequence=null
      ) {
        this.horizonalAxisName = horizonalAxisName;
        this.verticalAxisName = verticalAxisName;
        this.timeframe = timeframe == default(Duration) ? 5.seconds() : timeframe;
        sequence = sequence ?? DEFAULT_DIRECTION_SEQUENCE;
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
      IDisposableTracker tracker,
      DebugSequenceMouseData mouseData=null, Functional.Option<DebugSequenceDirectionData> directionDataOpt=default,
      DebugConsoleBinding binding=null, Functional.Option<KeyCodeWithModifiers> keyboardShortcutOpt = default,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      Option.ensureValue(ref directionDataOpt);
      Option.ensureValue(ref keyboardShortcutOpt);

      mouseData = mouseData ?? DEFAULT_MOUSE_DATA;

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
          var directions = Observable.everyFrame.collect(_ => {
            var horizontal = Input.GetAxisRaw(directionData.horizonalAxisName);
            var vertical = Input.GetAxisRaw(directionData.verticalAxisName);
            // Both are equal, can't decide.
            if (Math.Abs(horizontal - vertical) < 0.001f) return Functional.Option<Direction>.None;
            return
              Math.Abs(horizontal) > Math.Abs(vertical)
              ? F.some(horizontal > 0 ? Direction.Right : Direction.Left)
              : F.some(vertical > 0 ? Direction.Up : Direction.Down);
          }).changedValues();

          return
            directions
            .withinTimeframe(directionData.sequence.Count, directionData.timeframe)
            .filter(l => l.Select(t => t._1).SequenceEqual(directionData.sequence))
            .discardValue();
        }
      );

      var keyboardShortcutObs = keyboardShortcutOpt.fold(
        Observable<Unit>.empty,
        kc => Observable.everyFrame.filter(_ => kc.getKeyDown)
      );

      var obs = mouseObs.joinAll(new [] {directionObs, keyboardShortcutObs});
      obs.subscribe(tracker, _ => instance.show(binding));
      return obs;
    }

    public ISubscription register(IDisposableTracker tracker, Command command) {
      var list = commands.get(command.cmdGroup).getOrElse(() => {
        var lst = new List<Command>();
        commands[command.cmdGroup] = lst;
        return lst;
      });
      list.Add(command);
      var sub = new Subscription(() => list.Remove(command));
      tracker.track(sub, callerMemberName: $"DConsole register: {command.cmdGroup}/{command.name}");
      return sub;
    }

    public DConsoleRegistrar registrarFor(string prefix, IDisposableTracker tracker) =>
      new DConsoleRegistrar(this, prefix, tracker);

    public void registerHashSet<A>(
      string name, IDisposableTracker tracker, Ref<ImmutableHashSet<A>> pv, IEnumerable<A> options
    ) {
      var r = registrarFor(name, tracker);
      r.register("List", () => pv.value.asDebugString());
      r.register("Clear", () => {
        pv.value = ImmutableHashSet<A>.Empty;
        return pv.value.asDebugString();
      });
      foreach (var f in options) {
        r.register($"{f}?", () => pv.value.Contains(f));
        r.register($"Toggle {f}", () => {
          pv.value = pv.value.toggle(f);
          return $"in set={pv.value.Contains(f)}";
        });
      }
    }
    
    public void show(DebugConsoleBinding binding = null) {
      binding = binding ? binding : Resources.Load<DebugConsoleBinding>("Debug Console Prefab");

      destroy();
      onShow?.Invoke(this);
      onShow = null;

      var view = binding.clone();
      Object.DontDestroyOnLoad(view);

      var currentGroupButtons = ImmutableList<ButtonBinding>.Empty;
      setupList(view.commands, () => currentGroupButtons);

      var commandGroups = commands.OrderBySafe(_ => _.Key).Select(commandGroup => {
        var button = addButton(view.buttonPrefab, view.commandGroups.holder.transform);
        button.text.text = commandGroup.Key;
        button.button.onClick.AddListener(() =>
          currentGroupButtons = showGroup(view, commandGroup.Key, commandGroup.Value)
        );
        return button;
      }).ToImmutableList();
      setupList(view.commandGroups, () => commandGroups);
      
      var logEntryPool = GameObjectPool.a(GameObjectPool.Init<VerticalLayoutLogEntry>.noReparenting(
        nameof(DConsole) + " log entry pool",
        () => view.logEntry.prefab.clone()
      ));

      var cache = new List<string>();
      var layout = new DynamicVerticalLayout.Init(
        view.dynamicLayout,
        // ReSharper disable once InconsistentlySynchronizedField
        logEntries
          .SelectMany(e => createEntries(e, logEntryPool, cache, view.lineWidth))
          .Select(_ => _.upcast(default(DynamicVerticalLayout.IElementData))),
        renderLatestItemsFirst: true
      );

      var logCallback = onLogMessageReceived(logEntryPool, cache);
      Application.logMessageReceivedThreaded += logCallback;
      view.closeButton.onClick.AddListener(destroy);

      current = new Instance(view, layout, logCallback, logEntryPool).some();
    }
    
    // DO NOT generate comparer and hashcode - we need reference equality for dynamic vertical layout!
    [Record(GenerateComparer = false, GenerateGetHashCode = false)]
    partial class DynamicVerticalLayoutLogElementData : DynamicVerticalLayout.IElementWithViewData {
      readonly GameObjectPool<VerticalLayoutLogEntry> pool;
      readonly VerticalLayoutLogEntry.Data data;
      
      public float height => 20;
      public Percentage width => new Percentage(1f);
      public Functional.Option<DynamicVerticalLayout.IElementWithViewData> asElementWithView => 
        this.some<DynamicVerticalLayout.IElementWithViewData>();

      public DynamicVerticalLayout.IElementView createItem(Transform parent) {
        var logEntry = pool.BorrowDisposable();
        logEntry.value.transform.SetParent(parent, false);
        return new VerticalLayoutLogEntry.Init(logEntry, data);      
      }
    }

    static void setupList(DebugConsoleListBinding listBinding, Func<ImmutableList<ButtonBinding>> contents) {
      listBinding.clearFilterButton.onClick.AddListener(() => listBinding.filterInput.text = "");
      listBinding.filterInput.onValueChanged.AddListener(value => {
        foreach (var button in contents()) {
          var active = button.text.text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
          button.gameObject.SetActive(active);
        }
      });
    }

    static ImmutableList<ButtonBinding> showGroup(
      DebugConsoleBinding view, string groupName, IEnumerable<Command> commands
    ) {
      view.commandGroupLabel.text = groupName;
      var commandsHolder = view.commands.holder;
      foreach (var t in commandsHolder.transform.children()) Object.Destroy(t.gameObject);
      return commands.Select(command => {
        var button = addButton(view.buttonPrefab, commandsHolder.transform);
        button.text.text = command.name;
        button.button.onClick.AddListener(() => command.run());
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
    ) {
      return (message, stackTrace, type) => {
        foreach (var instance in current) {
          ASync.OnMainThread(() => {
            foreach (var e in createEntries(
              new LogEntry(message, type), pool, resultsTo,
              instance.view.lineWidth
            )) instance.dynamicVerticalLayout.appendDataIntoLayoutData(e);
          });
        }
      };
    }

    public void destroy() {
      foreach (var instance in current) {
        Application.logMessageReceivedThreaded -= instance.logCallback;
        instance.dynamicVerticalLayout.Dispose();
        instance.pool.dispose(Object.Destroy);
        Object.Destroy(instance.view.gameObject);
      }
      current = current.none;
    }
  }

  public delegate Functional.Option<Obj> HasObjFunc<Obj>();

  public struct DConsoleRegistrar {
    public readonly DConsole console;
    public readonly string commandGroup;
    readonly IDisposableTracker tracker;

    public DConsoleRegistrar(DConsole console, string commandGroup, IDisposableTracker tracker) {
      this.console = console;
      this.commandGroup = commandGroup;
      this.tracker = tracker;
    }

    static readonly HasObjFunc<Unit> unitSomeFn = () => F.some(F.unit);

    public ISubscription register(string name, Action run) =>
      register(name, () => { run(); return F.unit; });
    public ISubscription register<A>(string name, Func<A> run) =>
      register(name, unitSomeFn, _ => run());
    public ISubscription register<A>(string name, Func<Future<A>> run) =>
      register(name, unitSomeFn, _ => run());
    public ISubscription register<Obj>(string name, HasObjFunc<Obj> objOpt, Action<Obj> run) =>
      register(name, objOpt, obj => { run(obj); return F.unit; });
    public ISubscription register<Obj, A>(string name, HasObjFunc<Obj> objOpt, Func<Obj, A> run) =>
      register(name, objOpt, obj => Future.successful(run(obj)));
    public ISubscription register<Obj, A>(string name, HasObjFunc<Obj> objOpt, Func<Obj, Future<A>> run) {
      var prefixedName = $"[DC|{commandGroup}]> {name}";
      return console.register(tracker, new DConsole.Command(commandGroup, name, () => {
        var opt = objOpt();
        if (opt.isSome) {
          var returnFuture = run(opt.get);

          void onComplete(A t) => Debug.Log($"{prefixedName} done: {t}");
          // Check perhaps it is completed immediately.
          returnFuture.value.voidFold(
            () => {
              Debug.Log($"{prefixedName} starting.");
              returnFuture.onComplete(onComplete);
            },
            onComplete
          );
        }
        else Debug.Log($"{prefixedName} not running: {typeof(Obj)} is None.");
      }));
    }

    public void registerToggle(string name, Ref<bool> r, string comment=null) =>
      registerToggle(name, () => r.value, v => r.value = v, comment);

    public void registerToggle(string name, Func<bool> getter, Action<bool> setter, string comment=null) {
      register($"{name}?", getter);
      register($"Toggle {name}", () => {
        setter(!getter());
        return comment == null ? getter().ToString() : $"{comment}: value={getter()}";
      });
    }
    
    public void registerToggleOpt(string name, Ref<Functional.Option<bool>> r, string comment=null) {
      register($"{name}?", () => r.value);
      register($"Clear {name}", () => r.value = F.none_);
      register($"Toggle {name}", () => {
        var current = r.value.getOrElse(false);
        r.value = F.some(!current);
        return comment == null ? r.value.ToString() : $"{comment}: value={r.value}";
      });
    }

    public void registerNumeric<A>(
      string name, Ref<A> a, Numeric<A> num, A step,
      ImmutableList<A> quickSetValues = null
    ) {
      register($"{name}?", () => a.value);
      register($"{name} += {step}", () => a.value = num.add(a.value, step));
      register($"{name} -= {step}", () => a.value = num.subtract(a.value, step));
      if (quickSetValues != null) {
        foreach (var value in quickSetValues)
          register($"{name} = {value}", () => a.value = value);
      }
    }

    public void registerNumeric<A>(
      string name, Ref<A> a, Numeric<A> num,
      ImmutableList<A> quickSetValues = null
    ) =>
      registerNumeric(name, a, num, num.fromInt(1), quickSetValues);

    public void registerNumericOpt<A>(
      string name, Ref<Functional.Option<A>> aOpt, A showOnNone, Numeric<A> num,
      ImmutableList<A> quickSetValues = null
    ) {
      register($"Clear {name}", () => aOpt.value = Functional.Option<A>.None);
      register($"{name} opt?", () => aOpt.value);
      registerNumeric(
        name, Ref.a(
          () => aOpt.value.getOrElse(showOnNone),
          v => aOpt.value = v.some()
        ), num, quickSetValues
      );
    }

    public void registerCountdown(string name, uint count, Action act) {
      var countdown = count;
      register(name, () => {
        countdown--;
        if (countdown == 0) {
          act();
          countdown = count;
          return $"{name} EXECUTED.";
        }
        return $"Press me {countdown} more times to execute.";
      });
    }

    public void registerEnum<A>(
      string name, Ref<A> reference, IEnumerable<A> enumerable, string comment = null
    ) {
      register($"{name}?", () => {
        var v = reference.value;
        return comment == null ? v.ToString() : $"{comment}: value={v}";
      });
      foreach (var a in enumerable)
        register($"{name}={a}", () => {
          reference.value = a;
          return comment == null ? a.ToString() : $"{comment}: value={a}";
        });
    }

    public static readonly ImmutableArray<bool> BOOLS = ImmutableArray.Create(true, false);
    static readonly Functional.Option<bool>[] OPT_BOOLS = {F.none<bool>(), F.some(false), F.some(true)};
    
    [PublicAPI]
    public void registerBools(string name, Ref<bool> reference, string comment = null) =>
      registerEnum(name, reference, BOOLS, comment);
    
    [PublicAPI]
    public void registerBools(string name, Ref<Functional.Option<bool>> reference, string comment = null) =>
      registerEnum(name, reference, OPT_BOOLS, comment);
  }
}
