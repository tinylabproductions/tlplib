using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_Rotation : SerializedTweenerWithTransformTarget<Vector3, Quaternion> {
    public Transform_Rotation() : base(
      TweenOps.quaternion, SerializedTweenerOps.Add.quaternion, SerializedTweenerOps.Extract.rotation,
      Defaults.vector3
    ) { }

    protected override TweenMutator<Quaternion, Transform> mutator => TweenMutators.rotation;
    protected override Quaternion convert(Vector3 value) => Quaternion.Euler(value);
  }
}