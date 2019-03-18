using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  class TagSelector : EditorWindow, IMB_OnGUI {
    [UsedImplicitly, MenuItem("TLP/Tools/Scene/Select tags and layers")]
    static void Init() {
      var window = GetWindow<TagSelector>("Tag and Layer Selector");
      window.init();
      window.Show();
    }

    bool inited;
    string[] tags, layerNames;
    int[] layers;

    void init() {
      var gos = findAllGameObjectsinScene().ToArray();
      tags = gos.Select(_ => _.tag).Distinct().ToArray();
      layers = gos.Select(_ => _.layer).Distinct().ToArray();
      layerNames = layers.Select(LayerMask.LayerToName).ToArray();
      inited = true;
    }

    int selectedLayer, selectedTag;
    bool layerEnabled, tagEnabled;

    public void OnGUI() {
      if (!inited) init();

      tagEnabled = EditorGUILayout.BeginToggleGroup("Tag", tagEnabled);
      GUI.enabled = tagEnabled;
      selectedTag = EditorGUILayout.Popup(selectedTag, tags);

      GUI.enabled = true;
      layerEnabled = EditorGUILayout.BeginToggleGroup("Layer", layerEnabled);

      GUI.enabled = layerEnabled;
      selectedLayer = EditorGUILayout.Popup(selectedLayer, layerNames);

      GUI.enabled = tagEnabled || layerEnabled;
      if (GUILayout.Button("Select")) {
        selectFilteredObjects();
      }

      if (GUILayout.Button("Refresh lists")) {
        init();
      }
      GUI.enabled = true;

      EditorGUILayout.LabelField($"Selected objects: {Selection.objects.Length}");
    }

    IEnumerable<GameObject> findAllGameObjectsinScene() =>
      SceneManager.GetActiveScene()
        .findComponentsOfTypeAll<Transform>()
        .Select(_ => _.gameObject);


    void selectFilteredObjects() {
      var objects = findAllGameObjectsinScene()
        .Where(go =>
          (!tagEnabled || go.CompareTag(tags[selectedTag]))
          && (!layerEnabled || go.layer == layers[selectedLayer])
        )
        .ToArray();
      Selection.objects = objects;
    }
  }
}