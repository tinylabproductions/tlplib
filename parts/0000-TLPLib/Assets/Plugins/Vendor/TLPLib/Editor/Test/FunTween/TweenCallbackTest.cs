using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.FunTween {
  using ActionsToExecute = ImmutableList<Tpl<float, bool, ICollection<bool>>>;
  public class TweenCallbackTest {
    const float MIDDLE = 0.5f, END = 1f;

    static readonly TweenCallback emptyCallback = new TweenCallback(_ => { });
    static ActionsToExecute emptyActions => ActionsToExecute.Empty;
    static ImmutableList<float> emptyOtherPoints => ImmutableList<float>.Empty;

    static readonly ICollection<bool> eventForwards = ImmutableList.Create(true);
    static readonly ICollection<bool> eventBackwards = ImmutableList.Create(false);
    static readonly ICollection<bool> noEvent = ImmutableList<bool>.Empty;

    static readonly Subject<bool> stateSubject = new Subject<bool>();

    static void testSingleCallbackAt(
      float insertCallbackAt, ImmutableList<float> otherPoints, ActionsToExecute actions
    ) {
      var tsb = TweenTimeline.Builder.create().insert(
        insertCallbackAt,
        new TweenCallback(_ => stateSubject.push(_.playingForwards))
      );
      foreach (var otherPoint in otherPoints) tsb.insert(otherPoint, emptyCallback);
      var ts = tsb.build();

      var lastInvocation = 0f;
      
      foreach (var action in actions) {
        // Expected result correlates to playingForwards
        // as we are using playingforwards as the state variable
        var (setTimeTo, playingForwards, testResult) = action;
        Action execute = () => {
          ts.setRelativeTimePassed(lastInvocation, setTimeTo, playingForwards, true);
          lastInvocation = setTimeTo;
        };
        execute.shouldPushTo(stateSubject).resultIn(testResult);
      }
    }

    [TestFixture]
    public class CallbackAtZero {
      public void testCallbackAtTheStartZeroDuration(ActionsToExecute testCases) => testSingleCallbackAt(
        insertCallbackAt: 0f,
        otherPoints: emptyOtherPoints,
        actions: testCases
      );

      public void testCallbackAtTheStartNonZeroDuration(ActionsToExecute testCases) => testSingleCallbackAt(
        insertCallbackAt: 0f,
        otherPoints: emptyOtherPoints.Add(END),
        actions: testCases
      );

      [Test]
      public void zeroDurationMoveToZero() => testCallbackAtTheStartZeroDuration(
        emptyActions.Add(F.t(0f, true, eventForwards))
      );

      [Test]
      public void zeroDurationMoveToZeroAndBackToZero() => testCallbackAtTheStartZeroDuration(
       emptyActions.Add(F.t(0f, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void nonZeroDurationMoveToEndAndBackToZero() => testCallbackAtTheStartNonZeroDuration(
        emptyActions.Add(F.t(END, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void zeroDurationMoveTwiceForward() => testCallbackAtTheStartZeroDuration(
        emptyActions.Add(F.t(0f, true, eventForwards)).Add(F.t(0f, true, noEvent))
      );

      [Test]
      public void nonZeroDurationMoveToZeroAndMoveToEnd() => testCallbackAtTheStartNonZeroDuration(
        emptyActions.Add(F.t(0f, true, eventForwards)).Add(F.t(END, true, noEvent))
      );
    }

    [TestFixture]
    public class CallbackAtEnd {
      public void testCallbackAtTheEnd(ActionsToExecute testCases) => testSingleCallbackAt(
        insertCallbackAt: END,
        otherPoints: emptyOtherPoints,
        actions: testCases
      );

      [Test]
      public void moveToEnd() => testCallbackAtTheEnd(
        emptyActions.Add(F.t(END, true, eventForwards))
      );

      [Test]
      public void moveToEndAndMoveBack() => testCallbackAtTheEnd(
        emptyActions.Add(F.t(END, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void twiceMoveToEnd() => testCallbackAtTheEnd(
        emptyActions.Add(F.t(END, true, eventForwards)).Add(F.t(END, true, noEvent))
      );
    }

    [TestFixture]
    public class CallbackInTheMiddle {
      public void testCallbackAtTheMiddle(ActionsToExecute testCases) => testSingleCallbackAt(
        insertCallbackAt: MIDDLE,
        otherPoints: emptyOtherPoints.Add(END),
        actions: testCases
      );

      [Test]
      public void moveToAndAndMoveToZero() => testCallbackAtTheMiddle(
         emptyActions.Add(F.t(END, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
       );

      [Test]
      public void moveToMiddleAndMoveToZero() => testCallbackAtTheMiddle(
        emptyActions.Add(F.t(MIDDLE, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void moveToMiddleAndMoveToEnd() => testCallbackAtTheMiddle(
        emptyActions.Add(F.t(MIDDLE, true, eventForwards)).Add(F.t(END, true, noEvent))
      );

      [Test]
      public void twiceMoveToMiddle() => testCallbackAtTheMiddle(
        emptyActions.Add(F.t(MIDDLE, true, eventForwards)).Add(F.t(MIDDLE, true, noEvent))
      );

      [Test]
      public void moveToMiddleMoveBackByZeroMoveToEnd() => testCallbackAtTheMiddle(
        emptyActions
        .Add(F.t(MIDDLE, true, eventForwards))
        .Add(F.t(0f, false, eventBackwards))
        .Add(F.t(END, true, eventForwards))
      );
    }
  }
}