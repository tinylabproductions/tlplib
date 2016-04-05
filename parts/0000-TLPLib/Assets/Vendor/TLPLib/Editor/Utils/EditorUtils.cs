using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class EditorUtils {
    public static IEnumerable<GameObject> getSceneObjects() {
      return Resources.FindObjectsOfTypeAll<GameObject>()
          .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                 && go.hideFlags == HideFlags.None);
    }
  }
}
