using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [AddComponentMenu("")]
  public class Ease_Punch : ComplexSerializedEase {
    [
      SerializeField, 
      Tooltip("Indicates how much will the punch vibrate")
    ] int _vibrato = 10;

    [
      SerializeField, Range(0, 1),
      Tooltip(
        @"Represents how much the vector will go beyond the starting position when bouncing backwards.
1 creates a full oscillation between the direction and the opposite decaying direction,
while 0 oscillates only between the starting position and the decaying direction"
      )
    ] float _elasticity = 1;

    protected override Ease createEase() => Eases.punch(vibrato: _vibrato, elasticity: _elasticity);
    public override string easeName => $"Punch(v: {_vibrato}, e: {_elasticity})";
  }
}