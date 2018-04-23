using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_LocalPosition : SerializedTweener<Vector3, Transform> {
    public Transform_LocalPosition() : base(TweenOps.vector3, TweenMutators.localPosition, Defaults.vector3) { }
  }
}