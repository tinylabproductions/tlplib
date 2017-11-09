using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.FunTween {
  using TestCases = ImmutableList<Tpl<float, bool, ICollection<bool>>>;
  public class TweenCallbackTest {
    const float MIDDLE = 0.5f, END = 1f;

    static readonly TweenCallback emptyCallback = new TweenCallback(_ => { });
    static TestCases emptyTestCases => TestCases.Empty;
    static ImmutableList<float> emptyOtherPoints => ImmutableList<float>.Empty;

    static readonly ICollection<bool> eventForwards = ImmutableList.Create(true);
    static readonly ICollection<bool> eventBackwards = ImmutableList.Create(false);
    static readonly ICollection<bool> noEvent = ImmutableList<bool>.Empty;

    static readonly Subject<bool> stateSubject = new Subject<bool>();

    static void testSingleCallbackAt(
      float insertCallbackAt, ImmutableList<float> otherPoints, TestCases testCases
    ) {
      var tsb = TweenSequence.Builder.create().insert(
        insertCallbackAt, 
        new TweenCallback(_ => stateSubject.push(_.playingForwards))
      );
      foreach (var otherPoint in otherPoints) tsb.insert(otherPoint, emptyCallback);
      var ts = tsb.build();

      foreach (var testCase in testCases) {
        var setTimeTo = testCase._1;
        // Expected result correlates to playingForwards
        // as we are using playingforwards as the state variable
        var playingForwards = testCase._2;
        var testResult = testCase._3;
        Action execute = () => ts.setRelativeTimePassed(setTimeTo, playingForwards);
        execute.shouldPushTo(stateSubject).resultIn(testResult);
      }
    }

    [TestFixture]
    public class CallbackAtZero {
      public void testCallbackAtTheStartZeroDuration(TestCases testCases) => testSingleCallbackAt(
        insertCallbackAt: 0f,
        otherPoints: emptyOtherPoints,
        testCases: testCases
      );

      public void testCallbackAtTheStartNonZeroDuration(TestCases testCases) => testSingleCallbackAt(
        insertCallbackAt: 0f,
        otherPoints: emptyOtherPoints.Add(END),
        testCases: testCases
      );

      [Test]
      public void zeroDurationMoveToZero() => testCallbackAtTheStartZeroDuration(
        emptyTestCases.Add(F.t(0f, true, eventForwards))
      );

      [Test]
      public void zeroDurationMoveToZeroAndBackToZero() => testCallbackAtTheStartZeroDuration(
       emptyTestCases.Add(F.t(0f, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void nonZeroDurationMoveToEndAndBackToZero() => testCallbackAtTheStartNonZeroDuration(
        emptyTestCases.Add(F.t(END, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void zeroDurationMoveTwiceForward() => testCallbackAtTheStartZeroDuration(
        emptyTestCases.Add(F.t(0f, true, eventForwards)).Add(F.t(0f, true, noEvent))
      );

      [Test]
      public void nonZeroDurationMoveToZeroAndMoveToEnd() => testCallbackAtTheStartNonZeroDuration(
        emptyTestCases.Add(F.t(0f, true, eventForwards)).Add(F.t(END, true, noEvent))
      );
    }

    [TestFixture]
    public class CallbackAtEnd {
      public void testCallbackAtTheEnd(TestCases testCases) => testSingleCallbackAt(
        insertCallbackAt: END,
        otherPoints: emptyOtherPoints,
        testCases: testCases
      );

      [Test]
      public void moveToEnd() => testCallbackAtTheEnd(
        emptyTestCases.Add(F.t(END, true, eventForwards))
      );

      [Test]
      public void moveToEndAndMoveBack() => testCallbackAtTheEnd(
        emptyTestCases.Add(F.t(END, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void twiceMoveToEnd() => testCallbackAtTheEnd(
        emptyTestCases.Add(F.t(END, true, eventForwards)).Add(F.t(END, true, noEvent))
      );
    }

    [TestFixture]
    public class CallbackInTheMiddle {
      public void testCallbackAtTheMiddle(TestCases testCases) => testSingleCallbackAt(
        insertCallbackAt: MIDDLE,
        otherPoints: emptyOtherPoints.Add(END),
        testCases: testCases
      );

      [Test]
      public void moveToAndAndMoveToZero() => testCallbackAtTheMiddle(
         emptyTestCases.Add(F.t(END, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
       );

      [Test]
      public void moveToMiddleAndMoveToZero() => testCallbackAtTheMiddle(
        emptyTestCases.Add(F.t(MIDDLE, true, eventForwards)).Add(F.t(0f, false, eventBackwards))
      );

      [Test]
      public void moveToMiddleAndMoveToEnd() => testCallbackAtTheMiddle(
        emptyTestCases.Add(F.t(MIDDLE, true, eventForwards)).Add(F.t(END, true, noEvent))
      );

      [Test]
      public void twiceMoveToMiddle() => testCallbackAtTheMiddle(
        emptyTestCases.Add(F.t(MIDDLE, true, eventForwards)).Add(F.t(MIDDLE, true, noEvent))
      );

      [Test]
      public void moveToMiddleMoveBackByZeroMoveToEnd() => testCallbackAtTheMiddle(
        emptyTestCases
        .Add(F.t(MIDDLE, true, eventForwards))
        .Add(F.t(0f, false, eventBackwards))
        .Add(F.t(END, true, eventForwards))
      );
    }
  }
}