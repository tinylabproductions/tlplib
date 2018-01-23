using AdvancedInspector;
using com.tinylabproductions.TLPLib.Editor.Utils;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.import {
  [CustomEditor(typeof(TextureToGameObjectImporter))]
  public class TextureToGameObjectImporterEditor : ScriptableEditor {
    public override void OnInspectorGUI() {
      var obj = (TextureToGameObjectImporter) target;
      
      base.OnInspectorGUI();
      EditorGUILayout.Space();
      if (GUILayout.Button("Generate")) {
        var width = obj.texture.width;
        var height = obj.texture.height;
        
        using (var progress = new EditorProgress("Generating objects")) {
          var pixels = progress.execute("Getting pixels", obj.texture.GetPixels);
          var dict = obj.pallete.toDict(_ => _.color.withAlpha(1), _ => _.gameObject);
          var parent = new GameObject(obj.holderGameObjectName).transform;

          progress.execute("Reading pixels", () => {
            for (var x = 0; x < width; x++) {
              for (var y = 0; y < height; y++) {
                var idx = y * width + x;
                // ReSharper disable once AccessToDisposedClosure
                progress.progress(idx, pixels.Length);
                var pixel = pixels[idx].withAlpha(1);
                if (dict.TryGetValue(pixel, out var go)) {
                  var position = obj.startPoint + new Vector3(x * obj.spacing.x, y * obj.spacing.y);
                  var instantiated = ((GameObject) PrefabUtility.InstantiatePrefab(go)).transform;
                  instantiated.parent = parent;
                  instantiated.position = position;
                }
              }
            }
          });
        }
      }
    }
  }
}