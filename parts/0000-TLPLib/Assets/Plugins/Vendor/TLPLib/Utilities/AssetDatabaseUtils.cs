#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class AssetDatabaseUtils {
    public static void withEveryPrefabOfType<A>(Act<A> f) {
      var prefabGuids = AssetDatabase.FindAssets("t:prefab");

      var prefabs = prefabGuids
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadMainAssetAtPath)
        .OfType<GameObject>();

      var list = new List<A>();

      foreach (var go in prefabs) {
        go.GetComponentsInChildren<A>(true, list);
        foreach (var component in list) {
          f(component);
        }
      }
    }
  }
}
#endif
