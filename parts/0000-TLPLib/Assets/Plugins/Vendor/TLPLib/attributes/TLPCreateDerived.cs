using System;
using Sirenix.OdinInspector;

namespace com.tinylabproductions.TLPLib.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  [DontApplyToListElements]
  public class TLPCreateDerivedAttribute : Attribute {}
}