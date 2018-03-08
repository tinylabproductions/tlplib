using System;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.validations {
  /// <summary>
  /// Marks a field, whose field's value supposed to be unique in the project.
  ///
  /// This attribute is intended to be used only on ScriptableObjects fields,
  /// but we are still using it on MonoBehaviours in some prefabs (WorldBinding._prefix),
  /// so for now we have to check them as well.
  /// </summary>
  [Record]
  public partial class UniqueValue : Attribute {
    // Checked field values are grouped by category
    public readonly string category;
  }
}