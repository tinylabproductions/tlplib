using System;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public enum Gender : byte { Male = 0, Female = 1 }

  public static class Gender_ {
    public static readonly Str<Gender> str = new Str.LambdaStr<Gender>(g => {
      switch (g) {
        case Gender.Male: return "male";
        case Gender.Female: return "female";
        default: throw new ArgumentOutOfRangeException(nameof(g), g, null);
      }
    });

    public static readonly ISerializedRW<Gender> serializedRW = SerializedRW.byte_.map(
      b => {
        switch (b) {
          case 0: return Gender.Male.some();
          case 1: return Gender.Female.some();
          default: return Option<Gender>.None;
        }
      },
      g => (byte) g
    );
  }
}