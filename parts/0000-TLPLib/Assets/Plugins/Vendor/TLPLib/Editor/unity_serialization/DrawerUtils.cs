using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.unity_serialization {
  public static class DrawerUtils {
    static readonly Dictionary<Color, GUIStyle> overlayStyleCache = new Dictionary<Color, GUIStyle>();

    public static GUIStyle overlayStyle() => overlayStyle(Color.blue);
    
    public static GUIStyle overlayStyle(Color color) {
      if (overlayStyleCache.TryGetValue(color, out var cached)) {
        return cached;
      }
      else {
        var overlay = new GUIStyle(EditorStyles.miniLabel) {
          alignment = TextAnchor.MiddleRight,
          contentOffset = new Vector2(-2, 0)
        };

        var c = EditorGUIUtility.isProSkin ? new Color(1 - color.r, 1 - color.g, 1 - color.b) : color;
        c.a = 0.75f;

        overlay.normal.textColor = c;
        overlayStyleCache.Add(color, overlay);
        return overlay;
      }
    }
    
    [PublicAPI]
    public static void twoFields(
      Rect position, GUIContent label, out Rect firstField, out Rect secondField, float ratio = 0.5f
    ) {
      position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
      var first = position.width * ratio;
      var second = position.width * (1f - ratio);
      firstField = new Rect(position.x, position.y, first, position.height);
      secondField = new Rect(position.x + first, position.y, second, position.height);
    }

    [PublicAPI]
    public static void twoFields(Rect position, out Rect firstField, out Rect secondField, float ratio = 0.5f) {
      var first = position.width * ratio;
      var second = position.width * (1f - ratio);
      firstField = new Rect(position.x, position.y, first, position.height);
      secondField = new Rect(position.x + first, position.y, second, position.height);
    }
  }

  [PublicAPI]
  public struct EditorIndent : IDisposable {
    readonly int initialLevel;

    public EditorIndent(int wantedLevel) {
      initialLevel = EditorGUI.indentLevel;
      EditorGUI.indentLevel = wantedLevel;
    }
    
    public void Dispose() { EditorGUI.indentLevel = initialLevel; }
  }
}