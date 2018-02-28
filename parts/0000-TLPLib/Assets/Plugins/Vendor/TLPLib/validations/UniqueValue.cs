using System;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.validations {
  /// <summary>
  /// Marks a field, whose field's value supposed to be unique in the project.
  /// Then ObjectValidator will validate it.
  /// Use this for unique identifiers such as string, byte[].
  /// </summary>
  [Record]
  public partial class UniqueValue : Attribute {
    public readonly string category;
  }
}