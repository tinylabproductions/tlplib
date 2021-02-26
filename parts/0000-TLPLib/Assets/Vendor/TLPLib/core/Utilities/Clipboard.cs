using UnityEngine;

namespace com.tinylabproductions.TLPLib.core.Utilities {
  /// <summary>
  /// A little helper for easier discovery of system clipboard functions.
  /// </summary>
  public static class Clipboard {
    /// <summary>Gives access to system clipboard.</summary>
    public static string value {
      get => GUIUtility.systemCopyBuffer;
      set => GUIUtility.systemCopyBuffer = value;
    }
  }
}