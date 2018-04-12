using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class boolRW : BaseRW<bool> {
    public const int LENGTH = 1;

    protected override DeserializeInfo<bool> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<bool>(BitConverter.ToBoolean(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(bool a) => Rope.a(BitConverter.GetBytes(a));
  }
}