using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  /**
   * Allows to easily instantiate things and not lose their prefab connection,
   * no matter if source is prefab, prefab instance or not a prefab.
   */
  public struct PrefabInstantiator {
    readonly Object source;
    readonly bool isPrefab;

    public PrefabInstantiator(GameObject _source) {
      Object source = _source;
      var prefabType = PrefabUtility.GetPrefabInstanceStatus(source);

      var isPrefabInstance =
        prefabType != PrefabInstanceStatus.NotAPrefab;

      if (isPrefabInstance) {
        source =
          #if UNITY_2018_2_OR_NEWER
            PrefabUtility.GetCorrespondingObjectFromSource(source)
          #else
            PrefabUtility.GetPrefabParent(source)
          #endif
        ;
        if (!source) throw new Exception(
          $"Can't look up prefab object (type: {prefabType}) from source {source}"
        );
      }

      isPrefab =
        isPrefabInstance ||
        PrefabUtility.GetPrefabAssetType(source) != PrefabAssetType.NotAPrefab;

      this.source = source;
    }

    public GameObject instantiate() {
      var instantiated = (GameObject) (
        isPrefab
          ? PrefabUtility.InstantiatePrefab(source)
          : Object.Instantiate(source)
      );
      if (!instantiated) throw new Exception(
        $"Failed to instantiate object from source ({source})!"
      );
      return instantiated;
    }
  }
}