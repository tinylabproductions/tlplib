using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class GuidExts {
    [PublicAPI]
    public static ulong asULong(this Guid g) {
      var bytes = g.ToByteArray();
      return unchecked (BitConverter.ToUInt64(bytes, 0) + BitConverter.ToUInt64(bytes, 8));
    }
  }
}