using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class TestTimeContext : ITimeContext {
    public readonly Duration timePerFrame;

    readonly RandomList<Tpl<Duration, Action>> actions = 
      new RandomList<Tpl<Duration, Action>>();

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
      actions.RemoveWhere(t => t.ua((runAt, act) => {
        var shouldRun = timePassed >= runAt;
        if (shouldRun) act();
        return shouldRun;
      }));
    }

    public Duration passedSinceStartup => timePassed;

    public void after(Duration duration, Action act) =>
      actions.Add(F.t(timePassed + duration, act));

    public void afterXFrames(int framesToSkip, Action act) =>
      actions.Add(F.t(timePassed + timePerFrame * framesToSkip, act));
  }
}