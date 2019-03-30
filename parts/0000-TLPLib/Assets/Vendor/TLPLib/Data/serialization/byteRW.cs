using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class byteRW : ISerializedRW<byte> {
    public Either<string, DeserializeInfo<byte>> deserialize(byte[] serialized, int startIndex) {
      if (serialized.tryGet(startIndex, out var b)) {
        return new DeserializeInfo<byte>(b, 1);
      }
      else {
        return $"Index out of bounds while deserializing byte: {startIndex}";
      }
    }

    public Rope<byte> serialize(byte a) => Rope.a(new [] {a});
  }
  
  class byteArrayRW : ISerializedRW<byte[]> {
    public Either<string, DeserializeInfo<byte[]>> deserialize(byte[] serialized, int startIndex) {
      var lengthDeserialize = SerializedRW.integer.deserialize(serialized, startIndex);
      if (!lengthDeserialize.rightValueOut(out var lengthInfo)) 
        return lengthDeserialize.__unsafeGetLeft;
      var dataStartIndex = startIndex + lengthInfo.bytesRead;
      var length = lengthInfo.value;
      try {
        var data = serialized.slice(dataStartIndex, length);
        return new DeserializeInfo<byte[]>(data, intRW.LENGTH + length);
      }
      catch (Exception e) {
        return $"byte[] deserialization at {startIndex}, dataStartIndex={dataStartIndex}, " +
               $"array length={length} threw {e}";
      }
    }

    public Rope<byte> serialize(byte[] arr) => SerializedRW.integer.serialize(arr.Length) + Rope.a(arr);
  }
}