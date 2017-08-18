﻿using System;
using System.Collections.Generic;
#if ADVANCED_INSPECTOR
using AdvancedInspector;
#endif
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Utilities {
  [Serializable] public class GraphicStyle {
    public Color graphicColor;
    public Color outlineColor;

    public bool gradient;
    bool gradientOn => gradient;
#if ADVANCED_INSPECTOR
    [Inspect(nameof(gradientOn))] 
#endif
    public Color gradientColor;

    public GraphicStyle(Color graphicColor, bool gradient, Color gradientColor, Color outlineColor) {
      this.graphicColor = graphicColor;
      this.gradient = gradient;
      this.gradientColor = gradientColor;
      this.outlineColor = outlineColor;
    }

    public void applyStyle(List<Graphic> graphics) {
      foreach (var graphic in graphics) {
        applyStyle(graphic);
      }
    }

    public void applyStyle(Graphic graphic) {
      foreach (var _outline in F.opt(graphic.GetComponent<Shadow>())) {
        _outline.effectColor = outlineColor;
      }
      graphic.color = graphicColor;
      foreach (var grad in F.opt(graphic.GetComponent<GradientSimple>())) {
        if (gradientOn) {
          grad.enabled = true;
          grad.topColor = graphicColor;
          grad.bottomColor = gradientColor;
          graphic.SetAllDirty();
        }
        else {
          grad.enabled = false;
        }
      }
    }
  }
}