using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class JointRW<A> : ISerializedRW<A> {
    readonly ISerializer<A> serializer;
    readonly IDeserializer<A> deserializer;

    public JointRW(ISerializer<A> serializer, IDeserializer<A> deserializer) {
      this.serializer = serializer;
      this.deserializer = deserializer;
    }

    public Either<string, DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex) =>
      deserializer.deserialize(serialized, startIndex);

    public Rope<byte> serialize(A a) =>
      serializer.serialize(a);
  }
}