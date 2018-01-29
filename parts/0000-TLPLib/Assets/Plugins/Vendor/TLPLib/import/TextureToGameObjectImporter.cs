using System;
using System.Collections.Generic;
using System.Text;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.import {
  [CreateAssetMenu(menuName = "Game/Texture To Game Object Importer Data")]
  public class TextureToGameObjectImporter : ScriptableObject {
    [Serializable]
    public class Data : IInspectorRunning {
      #region Unity Serialized Fields

#pragma warning disable 649
      public Color32 color;
      public GameObject gameObject;
      public GameObject[] otherObjects = F.emptyArray<GameObject>();
#pragma warning restore 649

      #endregion

      public IEnumerable<GameObject> gameObjects {
        get {
          yield return gameObject;
          foreach (var go in otherObjects)
            yield return go;
        }
      }

      public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(gameObject.nameOrNull());
        foreach (var o in otherObjects) {
          sb.Append(",");
          sb.Append(o.nameOrNull());
        }

        return $"#{color.toHex()} -> {sb}";
      }

      public void OnHeaderGUI() {
        EditorGUILayout.HelpBox(
          "If other objects are present, a random object will be spawned from " +
          "the first object and other objects.",
          MessageType.Info
        );
      }

      public void OnFooterGUI() {}
    }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    public Texture2D texture;
    public Vector3 startPoint;
    public Vector2 spacing;
    public Data[] pallete;
    public Color32[] ignoredColors;
    public string holderGameObjectName = "Generated from Texture";
    public ulong randomSeed = Rng.now.nextULongT._2;
#pragma warning restore 649

    #endregion
  }
}