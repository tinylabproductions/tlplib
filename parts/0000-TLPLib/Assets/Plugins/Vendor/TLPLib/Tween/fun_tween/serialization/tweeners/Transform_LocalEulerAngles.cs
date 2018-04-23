using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_LocalEulerAngles : SerializedTweener<Vector3, Transform> {
    public Transform_LocalEulerAngles() : base(TweenOps.vector3, TweenMutators.localEulerAngles, Defaults.vector3) { }
  }
}