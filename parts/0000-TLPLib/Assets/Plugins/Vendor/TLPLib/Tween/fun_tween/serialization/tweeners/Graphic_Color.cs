using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Graphic_Color : SerializedTweener<Color, Graphic> {
    protected override Act<Color, Graphic> mutator => TweenMutators.graphicColor;
    protected override TweenLerp<Color> lerp => TweenLerp.color;
  }
}