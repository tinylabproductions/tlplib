using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class Lambda<A> : ISerializedRW<A> {
    readonly Serialize<A> _serialize;
    readonly Deserialize<DeserializeInfo<A>> _deserialize;

    public Lambda(Serialize<A> serialize, Deserialize<DeserializeInfo<A>> deserialize) {
      _serialize = serialize;
      _deserialize = deserialize;
    }

    public Either<string, DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex) =>
      _deserialize(serialized, startIndex);

    public Rope<byte> serialize(A a) => _serialize(a);
  }
}