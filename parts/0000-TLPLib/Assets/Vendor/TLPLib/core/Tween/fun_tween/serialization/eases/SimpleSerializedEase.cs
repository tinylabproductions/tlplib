using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  /// <see cref="Eases"/>
  public enum SimpleSerializedEase : ushort {
    Linear = 0, 
    QuadIn = 10, 
    QuadOut = 11, 
    QuadInOut = 12,
    CubicIn = 20,
    CubicOut = 21,
    CubicInOut = 22,
    QuartIn = 30,
    QuartOut = 31,
    QuartInOut = 32,
    QuintIn = 40,
    QuintOut = 41,
    QuintInOut = 42,
    SineIn = 50,
    SineOut = 51,
    SineInOut = 52,
    CircularIn = 60,
    CircularOut = 61,
    CircularInOut = 62,
    ExpoIn = 70,
    ExpoOut = 71,
    ExpoInOut = 72,
    ElasticIn = 80,
    ElasticOut = 81,
    ElasticInOut = 82,
    BackIn = 90,
    BackOut = 91,
    BackInOut = 92,
    BounceIn = 100,
    BounceOut = 101,
    BounceInOut = 102
  }
  public static class SimpleSerializedEase_ {
    [PublicAPI] public static Ease toEase(this SimpleSerializedEase simple) {
      return simple switch {
        SimpleSerializedEase.Linear => Eases.linear,
        SimpleSerializedEase.QuadIn => Eases.quadIn,
        SimpleSerializedEase.QuadOut => Eases.quadOut,
        SimpleSerializedEase.QuadInOut => Eases.quadInOut,
        SimpleSerializedEase.CubicIn => Eases.cubicIn,
        SimpleSerializedEase.CubicOut => Eases.cubicOut,
        SimpleSerializedEase.CubicInOut => Eases.cubicInOut,
        SimpleSerializedEase.QuartIn => Eases.quartIn,
        SimpleSerializedEase.QuartOut => Eases.quartOut,
        SimpleSerializedEase.QuartInOut => Eases.quartInOut,
        SimpleSerializedEase.QuintIn => Eases.quintIn,
        SimpleSerializedEase.QuintOut => Eases.quintOut,
        SimpleSerializedEase.QuintInOut => Eases.quintInOut,
        SimpleSerializedEase.SineIn => Eases.sineIn,
        SimpleSerializedEase.SineOut => Eases.sineOut,
        SimpleSerializedEase.SineInOut => Eases.sineInOut,
        SimpleSerializedEase.CircularIn => Eases.circularIn,
        SimpleSerializedEase.CircularOut => Eases.circularOut,
        SimpleSerializedEase.CircularInOut => Eases.circularInOut,
        SimpleSerializedEase.ExpoIn => Eases.expoIn,
        SimpleSerializedEase.ExpoOut => Eases.expoOut,
        SimpleSerializedEase.ExpoInOut => Eases.expoInOut,
        SimpleSerializedEase.ElasticIn => Eases.elasticIn,
        SimpleSerializedEase.ElasticOut => Eases.elasticOut,
        SimpleSerializedEase.ElasticInOut => Eases.elasticInOut,
        SimpleSerializedEase.BackIn => Eases.backIn,
        SimpleSerializedEase.BackOut => Eases.backOut,
        SimpleSerializedEase.BackInOut => Eases.backInOut,
        SimpleSerializedEase.BounceIn => Eases.bounceIn,
        SimpleSerializedEase.BounceOut => Eases.bounceOut,
        SimpleSerializedEase.BounceInOut => Eases.bounceInOut,
        _ => throw new Exception($"Unknown ease {simple}!")
      };
    }
  }
}