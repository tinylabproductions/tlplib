using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class DateTimeRW : BaseRW<DateTime> {
    protected override DeserializeInfo<DateTime> tryDeserialize(byte[] serialized, int startIndex) =>
      SerializedRW.lng.deserialize(serialized, startIndex).map(DateTime.FromBinary).get;

    public override Rope<byte> serialize(DateTime a) => SerializedRW.lng.serialize(a.ToBinary());
  }
}