using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_Position : SerializedTweener<Vector3, Transform> {
    protected override Act<Vector3, Transform> mutator => TweenMutators.position;
    protected override TweenLerp<Vector3> lerp => TweenLerp.vector3;
  }
}