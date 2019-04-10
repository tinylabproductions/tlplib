using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_Position : SerializedTweenerWithTransformTarget<Vector3> {
    public Transform_Position() : base(
      TweenOps.vector3, SerializedTweenerOps.Add.vector3, SerializedTweenerOps.Extract.position,
      TweenMutators.position, Defaults.vector3
    ) { }
  }
}