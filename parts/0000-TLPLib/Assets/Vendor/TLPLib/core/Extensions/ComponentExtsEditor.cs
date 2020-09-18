#if UNITY_EDITOR
using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI]
  public static class ComponentExtsEditor {
    public static void editPrefab<A>(this A prefabReference, Action<A> act) where A: Component {
      var assetPath = AssetDatabase.GetAssetPath(prefabReference);
      var contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
      try {
        var prefabInstance = contentsRoot.GetComponent<A>();
        act(prefabInstance);
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
      }
      finally {
        PrefabUtility.UnloadPrefabContents(contentsRoot);
      }
    }
  }
}
#endif