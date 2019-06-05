using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Transform_LocalEulerAngles : SerializedTweenerWithTransformTarget<Vector3> {
    public Transform_LocalEulerAngles() : base(
      TweenOps.vector3, SerializedTweenerOps.Add.vector3, SerializedTweenerOps.Extract.localEulerAngles, 
      TweenMutators.localEulerAngles, Defaults.vector3
    ) { }
  }
}