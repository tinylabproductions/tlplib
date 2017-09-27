using System;

namespace com.tinylabproductions.TLPLib.validations {
  /// <summary>
  /// Marks an IList that is supposed to be non-empty. 
  /// Then ObjectValidator can validate it. 
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class NonEmptyAttribute : Attribute {}
}