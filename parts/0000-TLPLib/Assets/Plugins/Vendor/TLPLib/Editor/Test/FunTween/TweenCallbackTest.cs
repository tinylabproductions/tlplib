using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.FunTween {
  [TestFixture]
  public class TweenCallbackTest {
    const float MIDDLE = 0.5f, END = 1f;

    static readonly TweenCallback emptyCallback = new TweenCallback(_ => { });
    static TweenSequence.Builder getBuilder => TweenSequence.Builder.create();
    static ImmutableList<Tpl<float, bool>> getEmptyTestCases => ImmutableList<Tpl<float, bool>>.Empty;
    static ImmutableList<float> getEmptyOtherPoints => ImmutableList<float>.Empty;

    static readonly ImmutableList<Tpl<float, bool>> toZeroAndBackToZero = getEmptyTestCases.Add(F.t(0f, true)).Add(F.t(0f, false));
    static readonly ImmutableList<Tpl<float, bool>> toEndAndBackToZero = getEmptyTestCases.Add(F.t(END, true)).Add(F.t(0f, false));
    static readonly ImmutableList<Tpl<float, bool>> toMiddleAndBackToZero = getEmptyTestCases.Add(F.t(MIDDLE, true)).Add(F.t(0f, false));

    static void testSingleCallbackAt(
      float changeOccursAt, ImmutableList<float> otherPoints,
      ImmutableList<Tpl<float, bool>> testCases
    ) {
      var state = F.none<bool>();
      var tsb = getBuilder.insert(
        changeOccursAt, 
        new TweenCallback(_ => state = _.playingForwards.some())
      );
      foreach (var otherPoint in otherPoints) tsb.insert(otherPoint, emptyCallback);
      var ts = tsb.build();
      
      foreach (var testCase in testCases) {
        var setTimeTo = testCase._1;
        // Expected result correlates to playingForwards
        // as we are using playingforwards as the state variable
        var playingForwards = testCase._2;

        ts.setRelativeTimePassed(setTimeTo, playingForwards);
        state.shouldBeSome(playingForwards);
      }
    }

    #region CallbackAtStart
    [Test]
    // is this test needed as we have 
    // "callbackAtStartForwardsAndBackWithZeroDuration", which is almost the same as this one?
    public void callbackAtStartZeroDuration() => testSingleCallbackAt(
      changeOccursAt: 0f,
      otherPoints: getEmptyOtherPoints,
      testCases: getEmptyTestCases.Add(F.t(0f, true))  
    );

    [Test]
    public void callbackAtStartForwardsAndBackWithZeroDuration() => testSingleCallbackAt(
     changeOccursAt: 0f,
     otherPoints: getEmptyOtherPoints,
     testCases: toZeroAndBackToZero
   );

    [Test]
    public void callbackAtStartForwardsAndBackWithNonZeroDuration() => testSingleCallbackAt(
      changeOccursAt: 0f,
      otherPoints: getEmptyOtherPoints.Add(END),
      testCases: toEndAndBackToZero
    );
    #endregion

    #region CallbackAtEnd
    [Test]
    public void callbackAtEnd() => testSingleCallbackAt(
     changeOccursAt: END,
     otherPoints: getEmptyOtherPoints,
     testCases: getEmptyTestCases.Add(F.t(END, true))
    );

    [Test]
    public void callbackAtEndForwardsAndBack() => testSingleCallbackAt(
      changeOccursAt: END,
      otherPoints: getEmptyOtherPoints,
      testCases: toEndAndBackToZero
    );
    #endregion

    #region CallbackInTheMiddle
    [Test]
    public void callbackInTheMiddleForwardsAndBack() => testSingleCallbackAt(
      changeOccursAt: MIDDLE,
      otherPoints: getEmptyOtherPoints.Add(END),
      testCases: toEndAndBackToZero
    );

    [Test]
    public void callbackInTheMiddlePlayUntilCallbackThenPlayBackwards() => testSingleCallbackAt(
      changeOccursAt: MIDDLE,
      otherPoints: getEmptyOtherPoints.Add(END),
      testCases: toMiddleAndBackToZero
    );
    #endregion
  }
}