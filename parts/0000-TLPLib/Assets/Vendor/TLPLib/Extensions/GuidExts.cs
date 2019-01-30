using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class GuidExts {
    [PublicAPI]
    public static ulong asULong(this Guid g) => g.ToByteArray().guidAsULong();
  }
}