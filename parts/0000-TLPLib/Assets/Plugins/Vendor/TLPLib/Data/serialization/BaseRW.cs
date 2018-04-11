using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  abstract class BaseRW<A> : ISerializedRW<A> {
    public Option<DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex) {
      try { return tryDeserialize(serialized, startIndex).some(); }
      catch (Exception) { return Option<DeserializeInfo<A>>.None; }
    }

    protected abstract DeserializeInfo<A> tryDeserialize(byte[] serialized, int startIndex);

    public abstract Rope<byte> serialize(A a);
  }
}