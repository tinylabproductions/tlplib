using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DConsole {
    public struct Command {
      public readonly string cmdGroup, name;
      public readonly Action run;

      public Command(string cmdGroup, string name, Action run) {
        this.cmdGroup = cmdGroup;
        this.name = name;
        this.run = run;
      }
    }

    struct Instance {
      public readonly DebugConsoleBinding view;

      public Instance(DebugConsoleBinding view) {
        this.view = view;
      }
    }

    public static DConsole instance { get; } = new DConsole();
    public static readonly ImmutableArray<bool> bools = ImmutableArray.Create(true, false);

    DConsole() {
      var r = registrarFor(nameof(DConsole));
      r.register("Self-test", () => "self-test");
      r.register("Future Self-test", () => Future.delay(Duration.fromSeconds(1), () => "after 1 s"));
    }

    public delegate void OnShow(DConsole console);

    readonly Dictionary<string, List<Command>> commands = new Dictionary<string, List<Command>>();
    public event OnShow onShow;

    Option<Instance> current = F.none<Instance>();

    public static readonly int[] DEFAULT_SEQUENCE = { 0, 1, 3, 2, 0, 2, 3, 1, 0 };

    public static IObservable<Unit> registerDebugSequence(
      DebugConsoleBinding binding=null, int width=2, int height=2, int[] sequence=null
    ) {
      binding = binding ?? Resources.Load<DebugConsoleBinding>("Debug Console Prefab");
      sequence = sequence ?? DEFAULT_SEQUENCE;

      var obs = new RegionClickObservable(width, height).sequenceWithinTimeframe(sequence, 3);
      obs.subscribe(_ => instance.show(binding));
      return obs;
    }

    public void register(Command command) {
      var list = commands.get(command.cmdGroup).getOrElse(() => {
        var lst = new List<Command>();
        commands[command.cmdGroup] = lst;
        return lst;
      });
      list.Add(command);
    }

    public DConsoleRegistrar registrarFor(string prefix) {
      return new DConsoleRegistrar(this, prefix);
    }

    public void show(DebugConsoleBinding binding) {
      destroy();
      onShow?.Invoke(this);
      onShow = null;

      var view = binding.clone();
      Object.DontDestroyOnLoad(view);
      foreach (var commandGroup in commands) {
        var button = addButton(view.buttonPrefab, view.commandGroupsHolder.transform);
        button.text.text = commandGroup.Key;
        button.button.onClick.AddListener(() => showGroup(view, commandGroup.Key, commandGroup.Value));
      }

      Application.logMessageReceivedThreaded += onLogMessageReceived;
      view.closeButton.onClick.AddListener(destroy);

      current = new Instance(view).some();
    }

    static void showGroup(DebugConsoleBinding view, string groupName, IEnumerable<Command> commands) {
      view.commandGroupLabel.text = groupName;
      foreach (var t in view.commandsHolder.transform.children()) Object.Destroy(t.gameObject);
      foreach (var command in commands) {
        var button = addButton(view.buttonPrefab, view.commandsHolder.transform);
        button.text.text = command.name;
        button.button.onClick.AddListener(() => command.run());
      }
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

    void onLogMessageReceived(string message, string stackTrace, LogType type) {
      foreach (var instance in current) {
        ASync.OnMainThread(() => {
          var entry = instance.view.logEntryPrefab.clone();
          var shortText = $"{DateTime.Now}  {type}  {message}";

          entry.text = shortText;
          entry.GetComponent<RectTransform>().SetParent(
            instance.view.logEntriesHolder.transform, worldPositionStays: false
          );
          entry.transform.SetAsFirstSibling();
        });
      }
    }

    public void destroy() {
      foreach (var instance in current) {
        Application.logMessageReceivedThreaded -= onLogMessageReceived;
        Object.Destroy(instance.view.gameObject);
      }
      current = current.none;
    }
  }

  public delegate Option<Obj> HasObjFn<Obj>();

  public struct DConsoleRegistrar {
    public readonly DConsole console;
    public readonly string commandGroup;

    public DConsoleRegistrar(DConsole console, string commandGroup) {
      this.console = console;
      this.commandGroup = commandGroup;
    }

    static readonly HasObjFn<Unit> unitSomeFn = () => F.some(F.unit);

    public void register(string name, Action run) {
      register(name, () => { run(); return F.unit; });
    }
    public void register<A>(string name, Fn<A> run) {
      register(name, unitSomeFn, _ => run());
    }
    public void register<A>(string name, Fn<Future<A>> run) {
      register(name, unitSomeFn, _ => run());
    }
    public void register<Obj>(string name, HasObjFn<Obj> objOpt, Act<Obj> run) {
      register(name, objOpt, obj => { run(obj); return F.unit; });
    }
    public void register<Obj, A>(string name, HasObjFn<Obj> objOpt, Fn<Obj, A> run) {
      register(name, objOpt, obj => Future.successful(run(obj)));
    }
    public void register<Obj, A>(string name, HasObjFn<Obj> objOpt, Fn<Obj, Future<A>> run) {
      var prefixedName = $"[DC|{commandGroup}]> {name}";
      console.register(new DConsole.Command(commandGroup, name, () => {
        var opt = objOpt();
        if (opt.isDefined) {
          var returnFuture = run(opt.get);
          Act<A> onComplete = t => Debug.Log($"{prefixedName} done: {t}");
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

    public void registerToggle(string name, Ref<bool> r) =>
      registerToggle(name, () => r.value, v => r.value = v);

    public void registerToggle(string name, Fn<bool> getter, Act<bool> setter) {
      register($"{name}?", getter);
      register($"{name}=true", () => setter(true));
      register($"{name}=false", () => setter(false));
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
  }
}
