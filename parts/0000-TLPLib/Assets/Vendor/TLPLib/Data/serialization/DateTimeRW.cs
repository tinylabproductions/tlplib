using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class DateTimeRW : BaseRW<DateTime> {
    protected override DeserializeInfo<DateTime> tryDeserialize(byte[] serialized, int startIndex) =>
      SerializedRW.lng.deserialize(serialized, startIndex)
        .mapRight(di => di.map(DateTime.FromBinary)).rightOrThrow;

    public override Rope<byte> serialize(DateTime a) => SerializedRW.lng.serialize(a.ToBinary());
  }
  
  class DateTimeMillisTimestampRW : BaseRW<DateTime> {
    protected override DeserializeInfo<DateTime> tryDeserialize(byte[] serialized, int startIndex) =>
      SerializedRW.lng.deserialize(serialized, startIndex)
        .mapRight(di => di.map(DateTimeExts.fromUnixTimestampInMilliseconds)).rightOrThrow;

    public override Rope<byte> serialize(DateTime a) => SerializedRW.lng.serialize(a.toUnixTimestampInMilliseconds());
  }
}