using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.gui {
  public class EditorGUI_ {
    /// <summary>Drop down that returns index of selected item.</summary>
    public static Option<int> IndexPopup(
      Rect position, Option<int> selectedIdx, string[] displayedOptions
    ) {
      const int NOT_SELECTED = -1;
      return
        EditorGUI.Popup(position, selectedIdx.getOrElse(NOT_SELECTED), displayedOptions)
        .mapVal(value => (value != NOT_SELECTED).opt(value));
    }
  }
}