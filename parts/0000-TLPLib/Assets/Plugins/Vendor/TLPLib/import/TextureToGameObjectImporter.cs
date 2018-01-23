using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.import {
  [CreateAssetMenu(menuName = "Game/Texture To Game Object Importer Data")]
  public class TextureToGameObjectImporter : ScriptableObject {
    [Serializable]
    public class Data {
      #region Unity Serialized Fields

#pragma warning disable 649
      public Color color;
      public GameObject gameObject;
#pragma warning restore 649

      #endregion
    }
    
    #region Unity Serialized Fields

#pragma warning disable 649
    public Texture2D texture;
    public Vector3 startPoint;
    public Vector2 spacing;
    public List<Data> pallete;
    public string holderGameObjectName = "Generated from Texture";
#pragma warning restore 649

    #endregion
  }
}