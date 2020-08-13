using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using pzd.lib.exts;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace com.tinylabproductions.TLPLib.Editor.extensions {
  [PublicAPI] public static class SpriteAtlasExts {
    public static ImmutableHashSet<Sprite> getPackedSprites(this SpriteAtlas atlas) {
      // https://docs.unity3d.com/ScriptReference/U2D.SpriteAtlasExtensions.Add.html
      //
      // At this moment, only Sprite, Texture2D and the Folder are allowed to be packable objects.
      // - "Sprite" will be packed directly.
      // - Each "sprite" in the "Texture2D" will be packed.
      // - Folder will be traversed. Each "Texture2D" child will be packed. Sub folder will be traversed.
      var packables = atlas.GetPackables();
      var sprites = ImmutableHashSet.CreateBuilder<Sprite>();
      var texturePaths = new List<string>();
      AssetDatabase.Refresh();
      var allImagePaths = AssetDatabase.GetAllAssetPaths().Select(_ => _.ToLowerInvariant()).Where(
        // this is not a complete list, but it is what we use for our project
        p => p.EndsWithFast(".psd") || p.EndsWithFast(".png")
      ).ToArray();
      foreach (var packable in packables) {
        switch (packable) {
          case Sprite sprite:
            sprites.Add(sprite);
            break;
          case Texture2D texture:
            addSpritesFromTexture(texture);
            break;
          case DefaultAsset folder:
            var path = AssetDatabase.GetAssetPath(folder).ToLowerInvariant();
            texturePaths.AddRange(allImagePaths.Where(p => p.StartsWithFast(path)));
            break;
          default:
            throw new Exception($"Unknown packable of type {packable.GetType().FullName}: {packable}");
        }
      }

      // AssetDatabase.FindAssets often fails to find all assets
      // var textures = 
      //   AssetDatabase.FindAssets("t:" + nameof(Texture2D), folders.ToArray())
      //   .Select(AssetDatabase.GUIDToAssetPath)
      //   .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
      //   .toHashSet();
      
      foreach (var texturePath in texturePaths.Distinct()) {
        addSpritesFromTexturePath(texturePath);
      }

      return sprites.ToImmutable();

      void addSpritesFromTexture(Texture2D texture) => addSpritesFromTexturePath(AssetDatabase.GetAssetPath(texture));
      
      void addSpritesFromTexturePath(string path) {
        var textureSprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
        foreach (var sprite in textureSprites) {
          sprites.Add(sprite);
        }
      }
    }  
  }
}