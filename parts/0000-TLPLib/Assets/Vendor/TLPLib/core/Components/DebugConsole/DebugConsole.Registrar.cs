using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.concurrent;
using pzd.lib.data;
using pzd.lib.dispose;
using pzd.lib.reactive;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.typeclasses;
using pzd.lib.utils;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.DebugConsole {
  [PublicAPI] public struct DConsoleRegistrar {
    public readonly DConsole console;
    public readonly string commandGroup;
    public readonly bool persistent;
    readonly ITracker tracker;

    public DConsoleRegistrar(
      DConsole console, string commandGroup, ITracker tracker, bool persistent
    ) {
      this.console = console;
      this.commandGroup = commandGroup;
      this.tracker = tracker;
      this.persistent = persistent;
    }

    static readonly HasObjFunc<Unit> unitSomeFn = () => Some.a(F.unit);

    public ISubscription register(
      string name, Action run, KeyCodeWithModifiers? shortcut = null, Func<bool> canShow = null
    ) => register(name, api => run(), shortcut, canShow);
    public ISubscription register(
      string name, Action<API> run, KeyCodeWithModifiers? shortcut = null, Func<bool> canShow = null
    ) => register(name, api => { run(api); return F.unit; }, shortcut, canShow);
    public ISubscription register<A>(
      string name, Func<A> run, KeyCodeWithModifiers? shortcut = null, Func<bool> canShow = null
    ) => register(name, api => run(), shortcut, canShow);
    public ISubscription register<A>(
      string name, Func<API, A> run, KeyCodeWithModifiers? shortcut = null, Func<bool> canShow = null
    ) => register(name, unitSomeFn, (api, _) => run(api), shortcut, canShow);
    public ISubscription register<A>(
      string name, Func<API, Future<A>> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) => register(name, unitSomeFn, (api, _) => run(api), shortcut, canShow);
    public ISubscription register<Obj>(
      string name, HasObjFunc<Obj> objOpt, Action<Obj> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) => register(name, objOpt, (api, obj) => run(obj), shortcut, canShow);
    public ISubscription register<Obj>(
      string name, HasObjFunc<Obj> objOpt, Action<API, Obj> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) => register(name, objOpt, (api, obj) => { run(api, obj); return F.unit; }, shortcut, canShow);
    public ISubscription register<Obj, A>(
      string name, HasObjFunc<Obj> objOpt, Func<Obj, A> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) => register(name, objOpt, (api, obj) => run(obj), shortcut, canShow);
    public ISubscription register<Obj, A>(
      string name, HasObjFunc<Obj> objOpt, Func<API, Obj, A> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) => register(name, objOpt, (api, obj) => Future.successful(run(api, obj)), shortcut, canShow);
    public ISubscription register<Obj, A>(
      string name, HasObjFunc<Obj> objOpt, Func<Obj, Future<A>> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) => register(name, objOpt, (api, obj) => run(obj), shortcut, canShow);
    public ISubscription register<Obj, A>(
      string name, HasObjFunc<Obj> objOpt, Func<API, Obj, Future<A>> run, KeyCodeWithModifiers? shortcut = null,
      Func<bool> canShow = null
    ) {
      var prefixedName = $"[DC|{commandGroup}]> {name}";
      return console.register(tracker, new DConsole.Command(commandGroup, name, shortcut.toOption(), api => {
        var opt = objOpt();
        if (opt.valueOut(out var obj)) {
          var returnFuture = run(api, obj);

          void onComplete(A t) => Debug.Log($"{prefixedName} done: {t}");
          // Check perhaps it is completed immediately.
          if (returnFuture.value.valueOut(out var a)) onComplete(a);
          else {
            Debug.Log($"{prefixedName} starting.");
            returnFuture.onComplete(onComplete);
          }
        }
        else Debug.Log($"{prefixedName} not running: {typeof(Obj)} is None.");
      }, canShow ?? (() => true), persistent: persistent));
    }

    public void registerToggle(
      string name, Ref<bool> r, string comment=null,
      KeyCodeWithModifiers? shortcut=null,
      Func<bool> canShow = null
    ) =>
      registerToggle(name, () => r.value, v => r.value = v, comment, shortcut, canShow);

    public void registerToggle(
      string name, Func<bool> getter, Action<bool> setter, string comment=null,
      KeyCodeWithModifiers? shortcut=null, Func<bool> canShow = null
    ) {
      register($"{name}?", getter, canShow: canShow);
      register($"Toggle {name}", shortcut: shortcut, run: () => {
        setter(!getter());
        return comment == null ? getter().ToString() : $"{comment}: value={getter()}";
      }, canShow: canShow);
    }
    
    public void registerToggleOpt(
      string name, Ref<Option<bool>> r, string comment=null,
      KeyCodeWithModifiers? shortcut=null, Func<bool> canShow = null
    ) {
      register($"{name}?", () => r.value, canShow: canShow);
      register($"Clear {name}", () => r.value = None._, canShow: canShow);
      register($"Toggle {name}", shortcut: shortcut, canShow: canShow, run: () => {
        var current = r.value.getOrElse(false);
        r.value = Some.a(!current);
        return comment == null ? r.value.ToString() : $"{comment}: value={r.value}";
      });
    }

    public void registerNumeric<A>(
      string name, Ref<A> a, Numeric<A> num, A step,
      ImmutableList<A> quickSetValues = null, Func<bool> canShow = null
    ) {
      register($"{name}?", () => a.value, canShow: canShow);
      register($"{name} += {step}", () => a.value = num.add(a.value, step), canShow: canShow);
      register($"{name} -= {step}", () => a.value = num.subtract(a.value, step), canShow: canShow);
      if (quickSetValues != null) {
        foreach (var value in quickSetValues)
          register($"{name} = {value}", () => a.value = value, canShow: canShow);
      }
    }

    public void registerNumeric<A>(
      string name, Ref<A> a, Numeric<A> num,
      ImmutableList<A> quickSetValues = null, Func<bool> canShow = null
    ) =>
      registerNumeric(name, a, num, num.fromInt(1), quickSetValues, canShow: canShow);

    public void registerNumericOpt<A>(
      string name, Ref<Option<A>> aOpt, A showOnNone, Numeric<A> num,
      ImmutableList<A> quickSetValues = null, Func<bool> canShow = null
    ) {
      register($"Clear {name}", () => aOpt.value = None._, canShow: canShow);
      register($"{name} opt?", () => aOpt.value, canShow: canShow);
      registerNumeric(
        name, Ref.a(
          () => aOpt.value.getOrElse(showOnNone),
          v => aOpt.value = v.some()
        ), num, quickSetValues, canShow: canShow
      );
    }

    public void registerCountdown(
      string name, uint count, Action run, KeyCodeWithModifiers? shortcut = null, Func<bool> canShow = null
    ) => registerCountdown(name, count, api => run(), shortcut, canShow);
    
    public void registerCountdown(
      string name, uint count, Action<API> run, KeyCodeWithModifiers? shortcut=null, Func<bool> canShow = null
    ) {
      var countdown = count;
      register(name, shortcut: shortcut, run: api => {
        countdown--;
        if (countdown == 0) {
          run(api);
          countdown = count;
          return $"{name} EXECUTED.";
        }
        return $"Press me {countdown} more times to execute.";
      }, canShow: canShow);
    }

    public void registerEnum<A>(
      string name, Ref<A> reference, IEnumerable<A> enumerable, string comment = null, Func<bool> canShow = null
    ) {
      register($"{name}?", () => {
        var v = reference.value;
        return comment == null ? v.ToString() : $"{comment}: value={v}";
      }, canShow: canShow);
      foreach (var a in enumerable)
        register($"{name}={a}", () => {
          reference.value = a;
          return comment == null ? a.ToString() : $"{comment}: value={a}";
        }, canShow: canShow);
    }

    public delegate bool IsSet<in A>(A value, A flag);
    public delegate A Set<A>(A value, A flag);
    public delegate A Unset<A>(A value, A flag);

    /// <param name="name"></param>
    /// <param name="reference"></param>
    /// <param name="isSet">Always <code>(value, flag) => (value & flag) != 0</code></param>
    /// <param name="set">Always <code>(value, flag) => value | flag</code></param>
    /// <param name="unset">Always <code>(value, flag) => value & ~flag</code></param>
    /// <param name="comment"></param>
    /// <param name="canShow"></param>
    public void registerFlagsEnum<A>(
      string name, Ref<A> reference,
      IsSet<A> isSet, Set<A> set, Unset<A> unset,
      string comment = null, Func<bool> canShow = null
    ) where A : Enum {
      var values = EnumUtils.GetValues<A>();
      register(
        $"{name}?", 
        () => 
          values
          .Select(a => $"{a}={isSet(reference.value, a)}")
          .OrderBySafe(_ => _)
          .mkString(", "),
        canShow: canShow
      );
      register(
        $"Set all {name}",
        () => reference.value = values.Aggregate(reference.value, (c, a) => set(c, a)), canShow: canShow
      );
      register(
        $"Clear all {name}",
        () => reference.value = values.Aggregate(reference.value, (c, a) => unset(c, a)), canShow: canShow
      );
      foreach (var a in values) {
        register($"{name}: toggle {a}", canShow: canShow, run: () =>
          reference.value = 
            isSet(reference.value, a) 
              ? unset(reference.value, a) 
              : set(reference.value, a)
        );
      }
    }

    public static readonly ImmutableArray<bool> BOOLS = ImmutableArray.Create(true, false);
    static readonly Option<bool>[] OPT_BOOLS = {F.none<bool>(), Some.a(false), Some.a(true)};
    
    public void registerBools(
      string name, Ref<bool> reference, string comment = null, Func<bool> canShow = null
    ) => registerEnum(name, reference, BOOLS, comment, canShow);
    
    public void registerBools(
      string name, Ref<Option<bool>> reference, string comment = null, Func<bool> canShow = null
    ) => registerEnum(name, reference, OPT_BOOLS, comment, canShow);
  }
}