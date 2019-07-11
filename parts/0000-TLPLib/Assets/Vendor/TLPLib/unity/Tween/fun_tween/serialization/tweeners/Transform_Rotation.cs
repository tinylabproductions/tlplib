using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_Rotation : SerializedTweener<Vector3, Quaternion, Transform> {
    public Transform_Rotation() : base(
      TweenOps.quaternion, SerializedTweenerOps.Add.quaternion, SerializedTweenerOps.Extract.rotation,
      Defaults.vector3
    ) { }

    protected override TweenMutator<Quaternion, Transform> mutator => TweenMutatorsU.rotation;
    protected override Quaternion convert(Vector3 value) => Quaternion.Euler(value);
  }
}