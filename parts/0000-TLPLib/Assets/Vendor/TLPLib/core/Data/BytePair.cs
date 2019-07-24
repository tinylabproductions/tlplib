using System;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.serialization;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// Essentially a tuple, but not generic, because <see cref="Tpl{P1,P2}"/> is a class on IL2CPP.
  /// </summary>
  [Record] public partial class BytePair {
    public readonly byte b1, b2;

    [PublicAPI] public void Deconstruct(out byte b1, out byte b2) {
      b1 = this.b1;
      b2 = this.b2;
    }
    
    [PublicAPI] public static readonly ISerializedRW<BytePair> rw =
      SerializedRW.byte_.and(SerializedRW.byte_, (b1, b2) => new BytePair(b1, b2), _ => _.b1, _ => _.b2);
  }
}