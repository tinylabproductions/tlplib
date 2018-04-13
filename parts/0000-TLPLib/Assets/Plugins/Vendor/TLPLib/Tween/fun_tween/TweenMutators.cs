using System;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class TweenMutators {
    #region Transform

    [PublicAPI] public static readonly Act<Vector3, Transform>
      position = (v, t) => t.position = v,
      localPosition = (v, t) => t.localPosition = v,
      localScale = (v, t) => t.localScale = v,
      localEulerAngles = (v, t) => t.localEulerAngles = v;

    #endregion

    #region RectTransform

    [PublicAPI] public static readonly Act<Vector2, RectTransform>
      anchoredPosition = (v, t) => t.anchoredPosition = v;

    #endregion

    #region Graphic

    [PublicAPI] public static readonly Act<Color, Graphic>
      graphicColor = (c, g) => g.color = c;

    [PublicAPI] public static readonly Act<float, Graphic> 
      graphicColorAlpha = (alpha, graphic) => {
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
      };

    #endregion

    #region Render Settings

    [PublicAPI] public static readonly Act<Color, Unit>
      globalFogColor = (v, _) => RenderSettings.fogColor = v;

    [PublicAPI] public static readonly Act<float, Unit>
      globalFogDensity = (v, _) => RenderSettings.fogDensity = v;

    #endregion

    #region Light

    [PublicAPI] public static readonly Act<Color, Light>
      lightColor = (v, o) => o.color = v;

    [PublicAPI] public static readonly Act<float, Light>
      lightIntensity = (v, o) => o.intensity = v;

    #endregion

    #region Renderer

    [PublicAPI] public static readonly Act<Color, Renderer>
      rendererTint = (v, o) => {
        foreach (var material in o.materials)
          material.color = v;
      };

    #endregion

    #region SpriteRenderer

    [PublicAPI] public static readonly Act<Color, SpriteRenderer>
      spriteRendererColor = (v, o) => o.color = v;

    #endregion

    #region Text

    [PublicAPI] public static readonly Act<Color, Text>
      textColor = (v, o) => o.color = v;

    #endregion
    
    [PublicAPI] public static readonly Act<float, Image>
      imageFillAmount = (v, o) => o.fillAmount = v;

    [PublicAPI] public static readonly Act<Color, Shadow>
      shadowEffectColor = (color, shadow) => shadow.effectColor = color;
  }
}