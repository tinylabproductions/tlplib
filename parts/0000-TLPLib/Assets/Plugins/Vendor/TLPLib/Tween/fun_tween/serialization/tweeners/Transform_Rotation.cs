using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_Rotation : SerializedTweener<Vector3, Quaternion, Transform> {
    public Transform_Rotation() : base(
      TweenOps.quaternion, TweenMutators.rotation, Quaternion.Euler, Defaults.vector3
    ) { }
  }
}