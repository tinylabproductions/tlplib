using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class intRW : BaseRW<int> {
    public const int LENGTH = 4;

    protected override DeserializeInfo<int> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<int>(BitConverter.ToInt32(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(int a) => Rope.a(BitConverter.GetBytes(a));
  }
}