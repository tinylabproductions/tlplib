using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
using static UnityEngine.GameObject;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  public class DConsole {
    public struct Command {
      public readonly string name;
      public readonly Act run;

      public Command(string name, Act run) {
        this.name = name;
        this.run = run;
      }
    }

    public static DConsole instance { get; } = new DConsole();

    DConsole() {
      register(new Command("Self-test to log", () => Log.rdebug("Debug console self-test.")));
    }

    readonly List<Command> _commands = new List<Command>();
    public IEnumerable<Command> commands => _commands;

    Option<DebugConsoleBinding> view = F.none<DebugConsoleBinding>();

    public static readonly int[] DEFAULT_SEQUENCE = { 0, 1, 3, 2, 0, 2, 3, 1, 0 };

    public static RegionClickObservable registerDebugSequence(
      DebugConsoleBinding binding,
      int[] sequence=null
    ) {
      sequence = sequence ?? DEFAULT_SEQUENCE;

      var go = new GameObject {name = "Debug Console initiator"};
      Object.DontDestroyOnLoad(go);

      var obs = go.AddComponent<RegionClickObservable>();
        obs.init(2, 2).sequenceWithinTimeframe(sequence, 3)
        .subscribe(_ => { instance.show(binding); });
      return obs;
    }

    public static Option<RegionClickObservable> registerDebugSequenceIfDebug(
      DebugConsoleBinding binding,
      int[] sequence = null
    ) {
      if (Log.isDebug) {
        Log.info("Registering debug console");
        return F.some(registerDebugSequence(binding, sequence));
      }
      else {
        Log.info("Debug console not registered, turn on debug log level.");
        return F.none<RegionClickObservable>();
      }
    }

    public void register(Command command) { _commands.Add(command); }

    public DConsoleRegistrar registrarFor(string prefix) {
      return new DConsoleRegistrar(this, prefix);
    }

    public void show(DebugConsoleBinding binding) {
      destroy();

      var view = binding.clone();

      foreach (var command in commands) {
        var button = view.buttonPrefab.clone();
        // Parent of RectTransform is being set with parent property. 
        // Consider using the SetParent method instead, with the worldPositionStays 
        // argument set to false. This will retain local orientation and scale rather 
        // than world orientation and scale, which can prevent common UI scaling issues.
        button.GetComponent<RectTransform>().
          SetParent(view.buttonHolder.transform, worldPositionStays: false);
        button.text.text = command.name;
        button.button.onClick.AddListener(() => command.run());
      }

      Application.logMessageReceived += onLogMessageReceived;

      view.closeButton.onClick.AddListener(destroy);

      this.view = F.some(view);
    }

    void onLogMessageReceived(string message, string stackTrace, LogType type) {
      view.each(v => {
        var entry = v.logEntryPrefab.clone();
        entry.text = $"{DateTime.Now}  {type}  {message}";
        entry.rectTransform.SetParent(
          v.logEntriesHolder.transform, worldPositionStays: false
        );
        entry.transform.SetAsFirstSibling();
      });
    }

    public void destroy() {
      view.each(v => {
        Application.logMessageReceived -= onLogMessageReceived;
        Object.Destroy(v.gameObject);
      });
      view = F.none<DebugConsoleBinding>();
    }
  }

  public struct DConsoleRegistrar {
    public readonly DConsole console;
    public readonly string prefix;

    public DConsoleRegistrar(DConsole console, string prefix) {
      this.console = console;
      this.prefix = prefix;
    }

    public void register(string name, Act run) {
      console.register(new DConsole.Command($"[{prefix}] {name}", run));
    }
  }
}
