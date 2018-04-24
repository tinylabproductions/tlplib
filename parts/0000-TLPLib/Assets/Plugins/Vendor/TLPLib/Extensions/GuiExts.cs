using System;

namespace Plugins.Vendor.TLPLib.Extensions {
  public static class GuiExts {
    public static ulong asULong(this Guid g) {
      var bytes = g.ToByteArray();
      return unchecked (BitConverter.ToUInt64(bytes, 0) + BitConverter.ToUInt64(bytes, 8));
    }
  }
}