using System.Collections.Generic;
using System.Linq;

using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Editor.Utils;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.exts;
using UnityEditor;
using UnityEngine;
using EditorUtils = com.tinylabproductions.TLPLib.Utilities.EditorUtils;

namespace com.tinylabproductions.TLPLib.import {
  [CustomEditor(typeof(TextureToGameObjectImporter))]
  public class TextureToGameObjectImporterEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      var obj = (TextureToGameObjectImporter) target;

      base.OnInspectorGUI();
      EditorGUILayout.Space();
      if (GUILayout.Button("Generate")) {
        if (!obj.texture) {
          EditorUtils.userInfo(
            "Texture not set!", "Please set the texture before generating.", Log.Level.ERROR
          );
          return;
        }

        var width = obj.texture.width;
        var height = obj.texture.height;
        var unknownColorsFound = new HashSet<Color32>();
        const byte maxAlpha = byte.MaxValue;
        var ignoredColors = obj.ignoredColors.Select(_ => _.with32Alpha(maxAlpha)).toHashSet();
        var dictV = obj.pallete
          .GroupBy(_ => _.color.with32Alpha(maxAlpha))
          .Select(group => {
            var count = group.Count();
            return (
              count == 1
              ? Either<string, KeyValuePair<Color32, GameObject[]>>.Right(F.kv(
                group.Key, group.SelectMany(_ => _.gameObjects).ToArray()
              ))
              : Either<string, KeyValuePair<Color32, GameObject[]>>.Left(
                $"#{group.Key.toHex()} should have 1 entry: {count} entries found."
              )
            ).asValidation();
          })
          .sequenceValidations();
        if (dictV.isLeft) {
          EditorUtils.userInfo(
            "Invalid pallete!",
            dictV.__unsafeGetLeft.mkString("\n"),
            Log.Level.ERROR
          );
          return;
        }
        var dict = dictV.__unsafeGetRight.toDict();

        using (var progress = new EditorProgress("Generating objects")) {
          var pixels = progress.execute("Getting pixels", obj.texture.GetPixels32);
          var parent = new GameObject(obj.holderGameObjectName).transform;

          progress.execute("Reading pixels", () => {
            var rng = new Rng(new Rng.Seed(obj.randomSeed));
            for (var y = 0; y < height; y++) {
              for (var x = 0; x < width; x++) {
                var idx = y * width + x;
                // ReSharper disable once AccessToDisposedClosure
                progress.progress(idx, pixels.Length);
                var pixel = pixels[idx].with32Alpha(maxAlpha);
                if (dict.TryGetValue(pixel, out var gameObjects)) {
                  var position = obj.startPoint + new Vector3(x * obj.spacing.x, y * obj.spacing.y);
                  var go = gameObjects.random(ref rng).getOrThrow($"No objects for #{pixel.toHex()} found!");
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

        if (unknownColorsFound.nonEmptyAllocating()) {
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