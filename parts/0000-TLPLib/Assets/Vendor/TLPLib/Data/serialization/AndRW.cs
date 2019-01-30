using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  [Record(GenerateComparer = false, GenerateGetHashCode = false, GenerateToString = false)]
  partial class AndRW2<A1, A2, B> : ISerializedRW<B> {
    readonly ISerializedRW<A1> a1RW;
    readonly ISerializedRW<A2> a2RW;
    readonly Fn<A1, A2, B> mapper;
    readonly Fn<B, A1> getA1;
    readonly Fn<B, A2> getA2;

    public Option<DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) {
      try {
        var a1Opt = a1RW.deserialize(serialized, startIndex);
        if (a1Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a1Info = a1Opt.__unsafeGetValue;
        var a2Opt = a2RW.deserialize(serialized, startIndex + a1Info.bytesRead);
        if (a2Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a2Info = a2Opt.__unsafeGetValue;
        var info = new DeserializeInfo<B>(
          mapper(a1Info.value, a2Info.value),
          a1Info.bytesRead + a2Info.bytesRead
        );
        return F.some(info);
      }
      catch (Exception) { return Option<DeserializeInfo<B>>.None; }
    }

    public Rope<byte> serialize(B b) =>
      a1RW.serialize(getA1(b)) + a2RW.serialize(getA2(b));
  }
  
  [Record(GenerateComparer = false, GenerateGetHashCode = false, GenerateToString = false)]
  partial class AndRW3<A1, A2, A3, B> : ISerializedRW<B> {
    readonly ISerializedRW<A1> a1RW;
    readonly ISerializedRW<A2> a2RW;
    readonly ISerializedRW<A3> a3RW;
    readonly Fn<A1, A2, A3, B> mapper;
    readonly Fn<B, A1> getA1;
    readonly Fn<B, A2> getA2;
    readonly Fn<B, A3> getA3;

    public Option<DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) {
      try {
        var a1Opt = a1RW.deserialize(serialized, startIndex);
        if (a1Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a1Info = a1Opt.__unsafeGetValue;
        var a2Opt = a2RW.deserialize(serialized, startIndex + a1Info.bytesRead);
        if (a2Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a2Info = a2Opt.__unsafeGetValue;
        var a3Opt = a3RW.deserialize(serialized, startIndex + a1Info.bytesRead + a2Info.bytesRead);
        if (a3Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a3Info = a3Opt.__unsafeGetValue;
        var info = new DeserializeInfo<B>(
          mapper(a1Info.value, a2Info.value, a3Info.value),
          a1Info.bytesRead + a2Info.bytesRead + a3Info.bytesRead
        );
        return F.some(info);
      }
      catch (Exception) { return Option<DeserializeInfo<B>>.None; }
    }

    public Rope<byte> serialize(B b) =>
      a1RW.serialize(getA1(b)) + a2RW.serialize(getA2(b)) + a3RW.serialize(getA3(b));
  }
  
  [Record(GenerateComparer = false, GenerateGetHashCode = false, GenerateToString = false)]
  partial class AndRW4<A1, A2, A3, A4, B> : ISerializedRW<B> {
    readonly ISerializedRW<A1> a1RW;
    readonly ISerializedRW<A2> a2RW;
    readonly ISerializedRW<A3> a3RW;
    readonly ISerializedRW<A4> a4RW;
    readonly Fn<A1, A2, A3, A4, B> mapper;
    readonly Fn<B, A1> getA1;
    readonly Fn<B, A2> getA2;
    readonly Fn<B, A3> getA3;
    readonly Fn<B, A4> getA4;

    public Option<DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) {
      try {
        var a1Opt = a1RW.deserialize(serialized, startIndex);
        if (a1Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a1Info = a1Opt.__unsafeGetValue;
        
        var a2Opt = a2RW.deserialize(serialized, startIndex + a1Info.bytesRead);
        if (a2Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a2Info = a2Opt.__unsafeGetValue;
        
        var a3Opt = a3RW.deserialize(serialized, startIndex + a1Info.bytesRead + a2Info.bytesRead);
        if (a3Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a3Info = a3Opt.__unsafeGetValue;
        
        var a4Opt = a4RW.deserialize(
          serialized, 
          startIndex + a1Info.bytesRead + a2Info.bytesRead + a3Info.bytesRead
        );
        if (a4Opt.isNone) return Option<DeserializeInfo<B>>.None;
        var a4Info = a4Opt.__unsafeGetValue;
        
        var info = new DeserializeInfo<B>(
          mapper(a1Info.value, a2Info.value, a3Info.value, a4Info.value),
          a1Info.bytesRead + a2Info.bytesRead + a3Info.bytesRead + a4Info.bytesRead
        );
        return F.some(info);
      }
      catch (Exception) { return Option<DeserializeInfo<B>>.None; }
    }

    public Rope<byte> serialize(B b) =>
      a1RW.serialize(getA1(b)) 
      + a2RW.serialize(getA2(b)) 
      + a3RW.serialize(getA3(b)) 
      + a4RW.serialize(getA4(b));
  }
}