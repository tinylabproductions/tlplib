using System;

namespace com.tinylabproductions.TLPLib.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class TLPCreateDerivedAttribute : Attribute {}
}