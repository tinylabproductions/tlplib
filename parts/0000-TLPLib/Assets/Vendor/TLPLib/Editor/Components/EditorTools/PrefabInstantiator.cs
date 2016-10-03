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
      var prefabType = PrefabUtility.GetPrefabType(source);

      var isPrefabInstance =
        prefabType == PrefabType.PrefabInstance ||
        prefabType == PrefabType.ModelPrefabInstance;

      if (isPrefabInstance) {
        source = PrefabUtility.GetPrefabParent(source);
        if (!source) throw new Exception(
          $"Can't look up prefab object (type: {prefabType}) from source {source}"
        );
      }

      isPrefab =
        isPrefabInstance ||
        prefabType == PrefabType.Prefab ||
        prefabType == PrefabType.ModelPrefab;

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