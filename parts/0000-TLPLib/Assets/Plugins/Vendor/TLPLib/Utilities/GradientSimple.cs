﻿using UnityEngine;
using System.Collections.Generic;
using Assets.Code.UI;

namespace com.tinylabproductions.TLPLib.Utilities {
  [AddComponentMenu("UI/Effects/Gradient")]
  public class GradientSimple : GradientBase {
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [SerializeField] public Color32 topColor = Color.white;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    public Color32 bottomColor = Color.black;

    public override void ModifyVertices(List<UIVertex> vertexList) {
      GradientHelper.modifyVertices(vertexList, (c, t) => Color32.Lerp(bottomColor, topColor, t));
    }
  }
}