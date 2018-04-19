using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  [AddComponentMenu("")]
  public class SerializedEasePunch : ComplexSerializedEase {
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

    Ease _ease;
    public override Ease ease => _ease ?? (_ease = Eases.punch(vibrato: _vibrato, elasticity: _elasticity));
  }
}