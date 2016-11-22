using System;

namespace com.tinylabproductions.TLPLib.validations {
  /**
   * Marks an ICollection that is supposed to be non-empty. 
   * Then MissingReferencesFinder can validate it. 
   **/
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class NonEmptyAttribute : Attribute {}
}