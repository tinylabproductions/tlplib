using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class floatRW : BaseRW<float> {
    public const int LENGTH = 4;

    protected override DeserializeInfo<float> tryDeserialize(byte[] serialized, int startIndex) =>
      new DeserializeInfo<float>(BitConverter.ToSingle(serialized, startIndex), LENGTH);

    public override Rope<byte> serialize(float a) => Rope.a(BitConverter.GetBytes(a));
  }
}