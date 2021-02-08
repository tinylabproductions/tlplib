using System;
using pzd.lib.dispose;
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

    /// <example>
    /// <code><![CDATA[
    /// using (EditorGUI_.indented()) {
    ///   ... your code ...
    /// }
    /// ]]></code>
    /// </example>
    public static ActionOnDispose indented(ushort by = 1) {
      EditorGUI.indentLevel += by;
      return new ActionOnDispose(() => EditorGUI.indentLevel -= by);
    }
  }
}