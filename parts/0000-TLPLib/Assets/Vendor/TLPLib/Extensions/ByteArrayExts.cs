using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ByteArrayExts {
    public static Try<int> toInt(this byte[] data, int startIndex = 0) {
      try { return F.scs(BitConverter.ToInt32(data, startIndex)); }
      catch (Exception e) { return F.err<int>(e); }
    }

    public static Try<ushort> toUShort(this byte[] data, int startIndex = 0) {
      try { return F.scs(BitConverter.ToUInt16(data, startIndex)); }
      catch (Exception e) { return F.err<ushort>(e); }
    }
  }
}