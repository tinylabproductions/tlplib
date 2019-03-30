using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class MappedSerializer<A, B> : ISerializer<B> {
    readonly ISerializer<A> aSerializer;
    readonly Func<B, A> mapper;

    public MappedSerializer(ISerializer<A> aSerializer, Func<B, A> mapper) {
      this.aSerializer = aSerializer;
      this.mapper = mapper;
    }

    public static Rope<byte> serialize(ISerializer<A> aSer, Func<B, A> mapper, B b) =>
      aSer.serialize(mapper(b));

    public Rope<byte> serialize(B b) => serialize(aSerializer, mapper, b);
  }

  class MappedDeserializer<A, B> : IDeserializer<B> {
    readonly IDeserializer<A> aDeserializer;
    readonly Func<A, Either<string, B>> mapper;

    public MappedDeserializer(IDeserializer<A> aDeserializer, Func<A, Either<string, B>> mapper) {
      this.aDeserializer = aDeserializer;
      this.mapper = mapper;
    }

    public Either<string, DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) =>
      deserialize(aDeserializer, mapper, serialized, startIndex);

    public static Either<string, DeserializeInfo<B>> deserialize(
      IDeserializer<A> aDeserializer, Func<A, Either<string, B>> mapper,
      byte[] serialized, int startIndex
    ) {
      var aInfoEither = aDeserializer.deserialize(serialized, startIndex);
      if (aInfoEither.leftValueOut(out var aErr)) return aErr;
      var aInfo = aInfoEither.__unsafeGetRight;
      var bEither = mapper(aInfo.value);
      if (bEither.leftValueOut(out var bErr)) return $"mapped deserializer failed in mapper: {bErr}";
      var bInfo = new DeserializeInfo<B>(bEither.__unsafeGetRight, aInfo.bytesRead);
      return bInfo;
    }
  }

  class MappedRW<A, B> : ISerializedRW<B> {
    readonly ISerializedRW<A> aRW;
    readonly Func<B, A> serializeConversion;
    readonly Func<A, Either<string, B>> deserializeConversion;

    public MappedRW(
      ISerializedRW<A> aRw, Func<B, A> serializeConversion,
      Func<A, Either<string, B>> deserializeConversion
    ) {
      aRW = aRw;
      this.serializeConversion = serializeConversion;
      this.deserializeConversion = deserializeConversion;
    }

    public Either<string, DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) =>
      MappedDeserializer<A, B>.deserialize(aRW, deserializeConversion, serialized, startIndex);

    public Rope<byte> serialize(B b) =>
      MappedSerializer<A, B>.serialize(aRW, serializeConversion, b);
  }
}