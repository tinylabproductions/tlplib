using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class byteRW : ISerializedRW<byte> {
    public Option<DeserializeInfo<byte>> deserialize(byte[] serialized, int startIndex) =>
      serialized.get(startIndex).map(b => new DeserializeInfo<byte>(b, 1));

    public Rope<byte> serialize(byte a) => Rope.a(new [] {a});
  }
  
  class byteArrayRW : ISerializedRW<byte[]> {
    public Option<DeserializeInfo<byte[]>> deserialize(byte[] serialized, int startIndex) {
      if (!SerializedRW.integer.deserialize(serialized, startIndex).valueOut(out var lengthInfo)) 
        return Option<DeserializeInfo<byte[]>>.None;
      var dataStartIndex = startIndex + lengthInfo.bytesRead;
      var length = lengthInfo.value;
      var data = serialized.slice(dataStartIndex, length);
      return F.some(new DeserializeInfo<byte[]>(data, length));
    }

    public Rope<byte> serialize(byte[] arr) => SerializedRW.integer.serialize(arr.Length) + Rope.a(arr);
  }
}