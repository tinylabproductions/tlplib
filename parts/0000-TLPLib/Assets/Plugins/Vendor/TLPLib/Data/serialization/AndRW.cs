using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  [Record(GenerateComparer = false, GenerateGetHashCode = false, GenerateToString = false)]
  partial class AndRW<A, B, C> : ISerializedRW<C> {
    readonly ISerializedRW<A> aRW;
    readonly ISerializedRW<B> bRW;
    readonly Fn<A, B, C> mapper;
    readonly Fn<C, A> getA;
    readonly Fn<C, B> getB;

    public Option<DeserializeInfo<C>> deserialize(byte[] serialized, int startIndex) {
      try {
        var aOpt = aRW.deserialize(serialized, startIndex);
        if (aOpt.isNone) return Option<DeserializeInfo<C>>.None;
        var aInfo = aOpt.get;
        var bOpt = bRW.deserialize(serialized, startIndex + aInfo.bytesRead);
        if (bOpt.isNone) return Option<DeserializeInfo<C>>.None;
        var bInfo = bOpt.get;
        var info = new DeserializeInfo<C>(
          mapper(aInfo.value, bInfo.value),
          aInfo.bytesRead + bInfo.bytesRead
        );
        return F.some(info);
      }
      catch (Exception) { return Option<DeserializeInfo<C>>.None; }
    }

    public Rope<byte> serialize(C c) =>
      aRW.serialize(getA(c)) + bRW.serialize(getB(c));
  }
}