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

    public Fn<string, int> actionCountsFn { get {
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

    void triggerActions() {
      actions.RemoveWhere(t => t.ua((runAt, act, name) => {
        var shouldRun = timePassed >= runAt;
        if (shouldRun) act();
        return shouldRun;
      }));
    }

    public Duration passedSinceStartup => timePassed;

    public void after(Duration duration, Action act, string name) =>
      actions.Add(F.t(timePassed + duration, act, name));

    public void afterXFrames(int framesToSkip, Action act, string name) =>
      actions.Add(F.t(timePassed + timePerFrame * framesToSkip, act, name));

    public override string ToString() {
      var acts = debugActions.Select(t => 
        $"{t._1.asString()}:{t._2.Select(_ => _ ?? "?").mkStringEnum()}"
      ).mkStringEnum();
      return $"{nameof(TestTimeContext)}[@{timePassed.asString()}, acts={acts}]";
    }

    public void setTimePassedAtLeast(Duration time) =>
      timePassed = Duration.comparable.max(timePassed, time);
  }
}