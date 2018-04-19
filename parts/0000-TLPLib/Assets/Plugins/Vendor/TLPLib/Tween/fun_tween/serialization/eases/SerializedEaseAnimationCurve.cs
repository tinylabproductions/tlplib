using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [AddComponentMenu("")]
  public class SerializedEaseAnimationCurve : ComplexSerializedEase {
    [SerializeField, NotNull] AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
    
    Ease _ease;
    public override Ease ease => _ease ?? (_ease = _curve.Evaluate);
  }
}