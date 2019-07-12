using System;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using JetBrains.Annotations;
using pzd.lib.serialization;

namespace com.tinylabproductions.TLPLib.Data {
  public enum Gender : byte { Male = 0, Female = 1 }
  [PublicAPI] public static class Gender_ {
    public static readonly Str<Gender> str = new Str.LambdaStr<Gender>(g => {
      switch (g) {
        case Gender.Male: return "male";
        case Gender.Female: return "female";
        default: throw new ArgumentOutOfRangeException(nameof(g), g, null);
      }
    });

    public static readonly ISerializedRW<Gender> serializedRW = SerializedRW.byte_.map<byte, Gender>(
      b => {
        switch (b) {
          case 0: return Gender.Male;
          case 1: return Gender.Female;
          default: return $"Unknown gender discriminator '{b}'";
        }
      },
      g => (byte) g
    );
  }
}