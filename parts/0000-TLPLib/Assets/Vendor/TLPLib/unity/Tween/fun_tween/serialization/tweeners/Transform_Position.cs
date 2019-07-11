using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_Position : SerializedTweener<Vector3, Transform> {
    public Transform_Position() : base(
      TweenOps.vector3, SerializedTweenerOps.Add.vector3, SerializedTweenerOps.Extract.position,
      TweenMutatorsU.position, Defaults.vector3
    ) { }
  }
}