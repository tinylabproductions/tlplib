using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.import {
  [CreateAssetMenu(menuName = "Game/Texture To Game Object Importer Data")]
  public class TextureToGameObjectImporter : ScriptableObject {
    [Serializable]
    public class Data {
      #region Unity Serialized Fields

#pragma warning disable 649
      public Color32 color;
      public GameObject gameObject;
#pragma warning restore 649

      #endregion

      public override string ToString() => 
        $"#{color.toHex()} -> {F.opt(gameObject).fold("none", _ => _.name)}";
    }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    public Texture2D texture;
    public Vector3 startPoint;
    public Vector2 spacing;
    public Data[] pallete;
    public Color32[] ignoredColors;
    public string holderGameObjectName = "Generated from Texture";
#pragma warning restore 649

    #endregion
  }
}