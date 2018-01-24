using System.Collections.Generic;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Editor.Utils;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;
using UnityEngine;
using EditorUtils = com.tinylabproductions.TLPLib.Utilities.EditorUtils;

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
        var unknownColorsFound = new HashSet<Color32>();
        const byte maxAlpha = byte.MaxValue;
        var ignoredColors = obj.ignoredColors.Select(_ => _.with32Alpha(maxAlpha)).toHashSet();
        
        using (var progress = new EditorProgress("Generating objects")) {
          var pixels = progress.execute("Getting pixels", obj.texture.GetPixels32);
          var dict = obj.pallete.toDict(_ => _.color.with32Alpha(maxAlpha), _ => _.gameObject);
          var parent = new GameObject(obj.holderGameObjectName).transform;

          progress.execute("Reading pixels", () => {
            for (var y = 0; y < height; y++) {
              for (var x = 0; x < width; x++) {
                var idx = y * width + x;
                // ReSharper disable once AccessToDisposedClosure
                progress.progress(idx, pixels.Length);
                var pixel = pixels[idx].with32Alpha(maxAlpha);
                if (dict.TryGetValue(pixel, out var go)) {
                  var position = obj.startPoint + new Vector3(x * obj.spacing.x, y * obj.spacing.y);
                  var instantiated = ((GameObject) PrefabUtility.InstantiatePrefab(go)).transform;
                  instantiated.parent = parent;
                  instantiated.position = position;
                }
                else if (!ignoredColors.Contains(pixel)) {
                  unknownColorsFound.Add(pixel);
                }
              }
            }
          });
        }

        if (unknownColorsFound.nonEmpty()) {
          EditorUtils.userInfo(
            "Found unknown colors!", level: Log.Level.ERROR,
            body: 
              "These colors were not defined:\n" + 
                unknownColorsFound.Select(_ => $"#{_.toHex()}").OrderBySafe(_ => _).mkString("\n")
          );
        }
      }
    }
  }
}