using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class ImmutableArrayDeserializer<A> : IDeserializer<ImmutableArray<A>> {
    readonly IDeserializer<A> deserializer;

    public ImmutableArrayDeserializer(
      IDeserializer<A> deserializer
    ) {
      this.deserializer = deserializer;
    }

    public Option<DeserializeInfo<ImmutableArray<A>>> deserialize(byte[] serialized, int startIndex) {
      try {
        var count = BitConverter.ToInt32(serialized, startIndex);
        var bytesRead = intRW.LENGTH;

        var builder = ImmutableArray.CreateBuilder<A>(count);
        var readIdx = startIndex + bytesRead;
        for (var idx = 0; idx < count; idx++) {
          var aOpt = deserializer.deserialize(serialized, readIdx);

          if (aOpt.isNone) {
            return Option<DeserializeInfo<ImmutableArray<A>>>.None;
          }
          var aInfo = aOpt.get;
          bytesRead += aInfo.bytesRead;
          readIdx += aInfo.bytesRead;
          builder.Add(aInfo.value);
        }
        return F.some(new DeserializeInfo<ImmutableArray<A>>(builder.MoveToImmutableSafe(), bytesRead));
      }
      catch (Exception) { return Option<DeserializeInfo<ImmutableArray<A>>>.None; }
    }
  }
}