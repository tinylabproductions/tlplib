using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  public class CollectionDeserializer<A, C> : IDeserializer<C> {
    readonly IDeserializer<A> deserializer;
    readonly CollectionBuilderKnownSizeFactory<A, C> builderKnownSizeFactory;

    [PublicAPI]
    public CollectionDeserializer(
      IDeserializer<A> deserializer, 
      CollectionBuilderKnownSizeFactory<A, C> builderKnownSizeFactory
    ) {
      this.deserializer = deserializer;
      this.builderKnownSizeFactory = builderKnownSizeFactory;
    }

    public Option<DeserializeInfo<C>> deserialize(byte[] serialized, int startIndex) {
      try {
        var count = BitConverter.ToInt32(serialized, startIndex);
        var bytesRead = intRW.LENGTH;

        var builder = builderKnownSizeFactory(count);
        var readIdx = startIndex + bytesRead;
        for (var idx = 0; idx < count; idx++) {
          var aOpt = deserializer.deserialize(serialized, readIdx);

          if (aOpt.isNone) {
            return Option<DeserializeInfo<C>>.None;
          }
          var aInfo = aOpt.get;
          bytesRead += aInfo.bytesRead;
          readIdx += aInfo.bytesRead;
          builder.add(aInfo.value);
        }
        return F.some(new DeserializeInfo<C>(builder.build(), bytesRead));
      }
      catch (Exception) { return Option<DeserializeInfo<C>>.None; }
    }
  }
}