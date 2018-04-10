using System;
using System.Text;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class stringRW : BaseRW<string> {
    static readonly Encoding encoding = Encoding.UTF8;

    // TODO: test
    protected override DeserializeInfo<string> tryDeserialize(byte[] serialized, int startIndex) {
      var length = BitConverter.ToInt32(serialized, startIndex);
      var str = encoding.GetString(serialized, startIndex + intRW.LENGTH, length);
      return new DeserializeInfo<string>(str, intRW.LENGTH + length);
    }

    public override Rope<byte> serialize(string a) {
      var serialized = encoding.GetBytes(a);
      var length = BitConverter.GetBytes(serialized.Length);
      return Rope.a(length, serialized);
    }
  }
}