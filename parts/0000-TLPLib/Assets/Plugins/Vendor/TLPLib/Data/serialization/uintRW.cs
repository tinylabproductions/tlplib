using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class uintRW : BaseRW<uint> {
    public const int LENGTH = 4;

    protected override DeserializeInfo<uint> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<uint>(BitConverter.ToUInt32(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(uint a) => Rope.a(BitConverter.GetBytes(a));
  }
}