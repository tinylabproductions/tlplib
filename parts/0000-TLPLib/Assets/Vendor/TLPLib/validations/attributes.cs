using System;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.validations {
  /// <summary>
  /// Marks an IList or String that is supposed to be non-empty.
  /// Then ObjectValidator can validate it.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), PublicAPI]
  public class NonEmptyAttribute : Attribute {}
  
  /// <summary>Marks a string field that is a unity tag.</summary>
  // Extends PropertyAttribute so it could be used in CustomPropertyDrawer
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property), PublicAPI]
  public class UnityTagAttribute : PropertyAttribute {}
}