using System;

namespace com.tinylabproductions.TLPLib.Plugins.Vendor.TLPLib.Utilities.Editor {
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
  public sealed class NotEmptyAttribute : Attribute { }
}
