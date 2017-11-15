using UnityEngine;
using UnityEditor;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Components.sorting_layer.Editor {
  public static class SortingLayerHelper {
    public static void drawSortingLayers(Renderer[] renderers) {
      if (renderers.Length == 0) return;

      var layers = SortingLayer.layers;
      var sortingLayerNames = layers.map(l => l.name);

      var oldId = renderers[0].sortingLayerID;
      var allIdsAreTheSame = true;
      foreach (var r in renderers) {
        if (r.sortingLayerID != oldId) {
          allIdsAreTheSame = false;
          break;
        }
      }

      var oldLayerIndex = -1;

      if (allIdsAreTheSame) {
        for (var i = 0; i < layers.Length; i++) {
          if (oldId == layers[i].id) oldLayerIndex = i;
        }
      }

      var newLayerIndex = EditorGUILayout.Popup("Sorting Layer", oldLayerIndex, sortingLayerNames);

      if (newLayerIndex != oldLayerIndex) {
        Undo.RecordObjects(renderers, "Edit Sorting Layer");
        var newId = layers[newLayerIndex].id;
        foreach (var r in renderers) {
          r.sortingLayerID = newId;
          EditorUtility.SetDirty(r);
        }
      }

      var oldOrder = renderers[0].sortingOrder;
      foreach (var r in renderers) {
        if (r.sortingOrder != oldOrder) oldOrder = int.MinValue;
      }

      var newSortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", oldOrder);
      if (newSortingLayerOrder != oldOrder) {
        Undo.RecordObjects(renderers, "Edit Sorting Order");
        foreach (var r in renderers) {
          r.sortingOrder = newSortingLayerOrder;
          EditorUtility.SetDirty(r);
        }
      }
    }
  }
}