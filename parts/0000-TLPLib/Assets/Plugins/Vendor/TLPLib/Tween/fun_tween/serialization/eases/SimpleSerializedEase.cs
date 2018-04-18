using com.tinylabproductions.TLPLib.Logger;
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
      switch (simple) {
        case SimpleSerializedEase.Linear: return Eases.linear;
        case SimpleSerializedEase.QuadIn: return Eases.quadIn;
        case SimpleSerializedEase.QuadOut: return Eases.quadOut;
        case SimpleSerializedEase.QuadInOut: return Eases.quadInOut;
        case SimpleSerializedEase.CubicIn: return Eases.cubicIn;
        case SimpleSerializedEase.CubicOut: return Eases.cubicOut;
        case SimpleSerializedEase.CubicInOut: return Eases.cubicInOut;
        case SimpleSerializedEase.QuartIn: return Eases.quartIn;
        case SimpleSerializedEase.QuartOut: return Eases.quartOut;
        case SimpleSerializedEase.QuartInOut: return Eases.quartInOut;
        case SimpleSerializedEase.QuintIn: return Eases.quintIn;
        case SimpleSerializedEase.QuintOut: return Eases.quintOut;
        case SimpleSerializedEase.QuintInOut: return Eases.quintInOut;
        case SimpleSerializedEase.SineIn: return Eases.sineIn;
        case SimpleSerializedEase.SineOut: return Eases.sineOut;
        case SimpleSerializedEase.SineInOut: return Eases.sineInOut;
        case SimpleSerializedEase.CircularIn: return Eases.circularIn;
        case SimpleSerializedEase.CircularOut: return Eases.circularOut;
        case SimpleSerializedEase.CircularInOut: return Eases.circularInOut;
        case SimpleSerializedEase.ExpoIn: return Eases.expoIn;
        case SimpleSerializedEase.ExpoOut: return Eases.expoOut;
        case SimpleSerializedEase.ExpoInOut: return Eases.expoInOut;
        case SimpleSerializedEase.ElasticIn: return Eases.elasticIn;
        case SimpleSerializedEase.ElasticOut: return Eases.elasticOut;
        case SimpleSerializedEase.ElasticInOut: return Eases.elasticInOut;
        case SimpleSerializedEase.BackIn: return Eases.backIn;
        case SimpleSerializedEase.BackOut: return Eases.backOut;
        case SimpleSerializedEase.BackInOut: return Eases.backInOut;
        case SimpleSerializedEase.BounceIn: return Eases.bounceIn;
        case SimpleSerializedEase.BounceOut: return Eases.bounceOut;
        case SimpleSerializedEase.BounceInOut: return Eases.bounceInOut;
        default:
          Log.d.error($"Unknown ease {simple}, returning linear!");
          return Eases.linear;
      }
    }
  }
}