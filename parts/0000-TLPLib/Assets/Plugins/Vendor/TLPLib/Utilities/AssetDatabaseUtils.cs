#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class AssetDatabaseUtils {
    public static void withEveryPrefabOfType<A>(Act<A> act) {
      var prefabGuids = AssetDatabase.FindAssets("t:prefab");

      var prefabs = prefabGuids
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadMainAssetAtPath)
        .OfType<GameObject>();

      var components = new List<A>();

      foreach (var go in prefabs) {
        go.GetComponentsInChildren<A>(includeInactive: true, results: components);
        foreach (var c in components) act(c);
      }
    }
  }
}
#endif
