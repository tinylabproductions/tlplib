using System;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.validations {
  /// <summary>
  /// Marks a field, whose field's value supposed to be unique in the project (like identifiers).
  /// Then ObjectValidator will validate it.
  /// This attribute is intended to be used only on ScriptableObjects fields,
  /// but we are still using MonoBehaviours in some prefabs,
  /// so for now we have to check them as well. (later we will show a warning)
  /// The ObjectValidator will not check for unique values with this attribute in scenes.
  /// Checked field values are grouped by category.
  /// </summary>
  [Record]
  public partial class UniqueValue : Attribute {
    public readonly string category;
  }
}