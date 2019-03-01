using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class ulongRW : BaseRW<ulong> {
    public const int LENGTH = 8;

    protected override DeserializeInfo<ulong> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<ulong>(BitConverter.ToUInt64(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(ulong a) => Rope.a(BitConverter.GetBytes(a));
  }
}