using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class byteRW : ISerializedRW<byte> {
    public Option<DeserializeInfo<byte>> deserialize(byte[] serialized, int startIndex) =>
      serialized.get(startIndex).map(b => new DeserializeInfo<byte>(b, 1));

    public Rope<byte> serialize(byte a) => Rope.a(new [] {a});
  }
}