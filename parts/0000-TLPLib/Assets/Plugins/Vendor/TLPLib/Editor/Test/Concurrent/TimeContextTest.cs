using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class TimeContextTestEvery {
    [Test]
    public void ItShouldRepeatTheAction() {
      var tc = new TestTimeContext();
      var invocations = 0;
      var duration = 3.seconds();
      tc.every(duration, () => invocations++);
      invocations.shouldEqual(0);

      tc.timePassed += duration;
      invocations.shouldEqual(1);

      tc.timePassed += duration / 2;
      invocations.shouldEqual(1);

      tc.timePassed += duration;
      invocations.shouldEqual(2);

      tc.timePassed += duration;
      invocations.shouldEqual(3);
    }

    [Test]
    public void ItShouldBeStoppable() {
      var tc = new TestTimeContext();
      var invocations = 0;
      var duration = 3.seconds();
      var cr = tc.every(duration, () => invocations++);
      invocations.shouldEqual(0);

      tc.timePassed += duration;
      invocations.shouldEqual(1);

      tc.timePassed += duration / 2;
      invocations.shouldEqual(1);

      tc.timePassed += duration;
      invocations.shouldEqual(2);

      var onFinishInvocations = 0;
      cr.onFinish += () => onFinishInvocations++;
      cr.stop();
      cr.finished.shouldBeTrue();
      onFinishInvocations.shouldEqual(1);

      cr.stop();
      cr.finished.shouldBeTrue();
      onFinishInvocations.shouldEqual(1);

      tc.timePassed += duration;
      invocations.shouldEqual(2);
    }

    [Test]
    public void ItShouldBeAbleToSelfStop() {
      var tc = new TestTimeContext();
      var invocations = 0;
      var duration = 3.seconds();
      var keepRunning = true;
      var cr = tc.every(duration, () => {
        invocations++;
        return keepRunning;
      });
      invocations.shouldEqual(0);

      tc.timePassed += duration;
      invocations.shouldEqual(1);

      tc.timePassed += duration / 2;
      invocations.shouldEqual(1);

      tc.timePassed += duration;
      invocations.shouldEqual(2);

      var onFinishInvocations = 0;
      cr.onFinish += () => onFinishInvocations++;

      keepRunning = false;
      tc.timePassed += duration;
      invocations.shouldEqual(3);

      cr.finished.shouldBeTrue();
      onFinishInvocations.shouldEqual(1);

      tc.timePassed += duration;
      invocations.shouldEqual(3);
      cr.finished.shouldBeTrue();
      onFinishInvocations.shouldEqual(1);

      cr.stop();
      cr.finished.shouldBeTrue();
      onFinishInvocations.shouldEqual(1);
    }
  }
}