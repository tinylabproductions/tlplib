using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class ushortRW : BaseRW<ushort> {
    public const int LENGTH = 2;

    protected override DeserializeInfo<ushort> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<ushort>(BitConverter.ToUInt16(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(ushort a) => Rope.a(BitConverter.GetBytes(a));
  }
}