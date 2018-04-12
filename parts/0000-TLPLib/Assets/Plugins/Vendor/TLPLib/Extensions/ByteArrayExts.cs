using System;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ByteArrayExts {
    [PublicAPI]
    public static Try<int> toInt(this byte[] data, int startIndex = 0) {
      try { return F.scs(BitConverter.ToInt32(data, startIndex)); }
      catch (Exception e) { return F.err<int>(e); }
    }

    [PublicAPI]
    public static Try<ushort> toUShort(this byte[] data, int startIndex = 0) {
      try { return F.scs(BitConverter.ToUInt16(data, startIndex)); }
      catch (Exception e) { return F.err<ushort>(e); }
    }
    
    /// <summary>
    /// Convert bytes to a hex string.
    /// </summary>
    [PublicAPI]
    public static string asHexString(this byte[] data) {
      var sb = new StringBuilder();

      foreach (var t in data) {
        sb.Append(Convert.ToString(t, 16).PadLeft(2, '0'));
      }

      return sb.ToString();
    }
  }
}