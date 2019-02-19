using System;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class TestTimeContext : ITimeContext {
    public readonly Duration timePerFrame;

    readonly RandomList<Tpl<Duration, Action, string>> actions =
      new RandomList<Tpl<Duration, Action, string>>();

    public IOrderedEnumerable<Tpl<Duration, ImmutableList<string>>> debugActions =>
      actions
        .GroupBy(_ => _._1)
        .Select(g => F.t(g.Key, g.Select(_ => _._3).ToImmutableList()))
        .OrderBy(_ => _._1.millis);

    public ImmutableDictionary<string, int> actionCounts =>
      actions
        .Where(_ => _._3 != null) // Dicts don't support null keys
        .GroupBy(_ => _._3)
        .ToImmutableDictionary(_ => _.Key, _ => _.Count());

    public Func<string, int> actionCountsFn { get {
      var dict = actionCounts;
      return key => dict.getOrElse(key, 0);
    } }

    Duration _timePassed = new Duration(0);
    public Duration timePassed {
      get { return _timePassed; }
      set {
        var oldValue = _timePassed;
        _timePassed = value;
        if (value > oldValue) triggerActions();
      }
    }

    public TestTimeContext(Duration timePerFrame) { this.timePerFrame = timePerFrame; }
    public TestTimeContext() : this(Duration.fromSeconds(1 / 60f)) {}

    public void reset() {
      actions.Clear();
      timePassed = 0.seconds();
    }

    void triggerActions() {
      // Actions might remove themselves as side effects, so we should first evaluate
      // all actions that should be run, then remove the ran actions if they are still
      // there.
      var toRun = actions.Where(t => t.ua((runAt, act, name) => {
        var shouldRun = timePassed >= runAt;
        return shouldRun;
      })).ToList();
      foreach (var t in toRun) t._2();
      actions.RemoveWhere(toRun.Contains);
    }

    public Duration passedSinceStartup => timePassed;

    public Coroutine after(Duration duration, Action act, string name) {
      TestTimeContextCoroutine cr = null;
      // ReSharper disable once PossibleNullReferenceException
      var entry = F.t(timePassed + duration, (Action) (() => cr.timeHit()), name);
      cr = new TestTimeContextCoroutine(() => actions.Remove(entry));
      cr.onFinish += act;
      actions.Add(entry);
      return cr;
    }

    public Coroutine afterXFrames(int framesToSkip, Action act, string name) =>
      after(timePerFrame * framesToSkip, act, name);

    public override string ToString() {
      var acts = debugActions.Select(t =>
        $"{t._1.asString()}:{t._2.Select(_ => _ ?? "?").mkStringEnum()}"
      ).mkStringEnum();
      return $"{nameof(TestTimeContext)}[@{timePassed.asString()}, acts={acts}]";
    }

    public void setTimePassedAtLeast(Duration time) =>
      timePassed = Duration.comparable.max(timePassed, time);
  }

  class TestTimeContextCoroutine : Coroutine {
    public event Action onFinish;
    public bool finished { get; private set; }

    readonly Action onDispose;

    public TestTimeContextCoroutine(Action onDispose) { this.onDispose = onDispose; }

    public void timeHit() {
      finished = true;
      onFinish?.Invoke();
    }

    public void Dispose() {
      onDispose();
      finished = true;
    }
  }
}