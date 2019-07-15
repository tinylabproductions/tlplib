using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.AssetTools {
  public class GUIDLookupWindow : EditorWindow, IMB_OnGUI {
    [MenuItem("TLP/Window/GUID lookup")]
    public static void init() {
      // Get existing open window or if none, make a new one:
      var window = GetWindow<GUIDLookupWindow>("GUID lookup");
      window.Show();
    }

    string guidStr;

    public void OnGUI() {
      guidStr = EditorGUILayout.TextField("GUID:", guidStr);

      foreach (var guid in guidStr.nonEmptyOpt(trim: true)) {
        GUILayoutUtility.GetRect(12f, 12f); // Space

        var objPathOpt = F.opt(AssetDatabase.GUIDToAssetPath(guid)).filter(_ => !string.IsNullOrEmpty(_));

        if (objPathOpt.isNone) {
          EditorGUILayout.HelpBox("Nothing found by this GUID.", MessageType.Info);
        }
        else {
          var objPath = objPathOpt.__unsafeGet;
          var obj = AssetDatabase.LoadAssetAtPath<Object>(objPath);

          EditorGUILayout.PrefixLabel($"Found at path:");
          EditorGUILayout.SelectableLabel(objPath);

          EditorGUILayout.ObjectField("Asset:", obj, typeof(Object), allowSceneObjects: false);
        }
      }
    }
  }
}