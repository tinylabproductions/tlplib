using System;
using com.tinylabproductions.TLPLib.Collection;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class GuidRW : BaseRW<Guid> {
    protected override DeserializeInfo<Guid> tryDeserialize(byte[] b, int startIndex) {
      const int GUID_SIZE = 16;
      // Copied from new Guid(byte[]) source.
      var _a = b[startIndex + 3] << 24 | b[startIndex + 2] << 16 | b[startIndex + 1] << 8 | b[startIndex + 0];
      var _b = (short) (b[startIndex + 5] << 8 | b[startIndex + 4]);
      var _c = (short) (b[startIndex + 7] << 8 | b[startIndex + 6]);
      var _d = b[startIndex + 8];
      var _e = b[startIndex + 9];
      var _f = b[startIndex + 10];
      var _g = b[startIndex + 11];
      var _h = b[startIndex + 12];
      var _i = b[startIndex + 13];
      var _j = b[startIndex + 14];
      var _k = b[startIndex + 15];
      var guid = new Guid(a: _a, b: _b, c: _c, d: _d, e: _e, f: _f, g: _g, h: _h, i: _i, j: _j, k: _k);
      return new DeserializeInfo<Guid>(guid, GUID_SIZE);
    }

    public override Rope<byte> serialize(Guid a) => Rope.a(a.ToByteArray());
  }
}