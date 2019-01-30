#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class TagIconManager {

    public enum LabelIcon {
      Gray,
      Blue,
      Teal,
      Green,
      Yellow,
      Orange,
      Red,
      Purple
    }

    public enum Icon {
      CircleGray,
      CircleBlue,
      CircleTeal,
      CircleGreen,
      CircleYellow,
      CircleOrange,
      CircleRed,
      CirclePurple,
      DiamondGray,
      DiamondBlue,
      DiamondTeal,
      DiamondGreen,
      DiamondYellow,
      DiamondOrange,
      DiamondRed,
      DiamondPurple
    }

    static GUIContent[] labelIcons;
    static GUIContent[] largeIcons;

    public static void setIcon(GameObject gObj, LabelIcon icon) {
      if (labelIcons == null) {
        labelIcons = getTextures("sv_label_", string.Empty, 0, 8);
      }

      setIcon(gObj, labelIcons[(int) icon].image as Texture2D);
    }

    public static void setIcon(GameObject gObj, Icon icon) {
      if (largeIcons == null) {
        largeIcons = getTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
      }
      setIcon(gObj, largeIcons[(int) icon].image as Texture2D);
    }

    static void setIcon(GameObject gObj, Texture2D texture) {
      var ty = typeof(EditorGUIUtility);
      var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
      mi.Invoke(null, new object[] {gObj, texture});
    }

    static GUIContent[] getTextures(string baseName, string postFix, int startIndex, int count) {
      var guiContentArray = new GUIContent[count];
      for (var index = 0; index < count; ++index) {
        guiContentArray[index] = EditorGUIUtility.IconContent(baseName + (startIndex + index) + postFix);
      }
      return guiContentArray;
    }
  }
}
#endif