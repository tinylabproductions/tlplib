#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class AssetDatabaseUtils {
    public static IEnumerable<A> getPrefabsOfType<A>() {
      var prefabGuids = AssetDatabase.FindAssets("t:prefab");

      var prefabs = prefabGuids
        .Select(loadMainAssetByGuid)
        .OfType<GameObject>();

      var components = new List<A>();

      foreach (var go in prefabs) {
        go.GetComponentsInChildren<A>(includeInactive: true, results: components);
        foreach (var c in components) yield return c;
      }
    }

    /// <summary>
    /// Sometimes Unity fails to find scriptable objects using the t: selector.
    /// 
    /// Our known workaround:
    /// 1. Open asset references window.
    /// 2. Find all instances of your scriptable object.
    /// 3. Show Actions > Set Dirty
    /// 4. Save project.
    /// 5. Profit!
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <returns></returns>
    public static IEnumerable<A> getScriptableObjectsOfType<A>() where A : ScriptableObject =>
      AssetDatabase.FindAssets($"t:{typeof(A).Name}")
      .Select(loadMainAssetByGuid)
      .OfType<A>();

    public static Object loadMainAssetByGuid(string guid) =>
      AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
  }
}
#endif
