using System;

namespace com.tinylabproductions.TLPLib {
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
  public sealed class NotNullAttribute : Attribute { }
}