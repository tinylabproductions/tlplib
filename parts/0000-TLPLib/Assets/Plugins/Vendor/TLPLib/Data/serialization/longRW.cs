using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class longRW : BaseRW<long> {
    public const int LENGTH = 8;

    protected override DeserializeInfo<long> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<long>(BitConverter.ToInt64(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(long a) => Rope.a(BitConverter.GetBytes(a));
  }
}