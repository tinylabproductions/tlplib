using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.path;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public delegate void TweenMutator<in PropertyType, in TargetType>(
    PropertyType value, TargetType target, bool isRelative
  );
  public static class TweenMutators {
    #region Transform

    [PublicAPI] public static readonly TweenMutator<Vector3, Transform>
      position = (v, t, r) => { if (r) t.position += v; else t.position = v; },
      localPosition = (v, t, r) => { if (r) t.localPosition += v; else t.localPosition = v; },
      localScale = (v, t, r) => { if (r) t.localScale += v; else t.localScale = v; },
      localEulerAngles = (v, t, r) => { if (r) t.localEulerAngles += v; else t.localEulerAngles = v; };
    
    [PublicAPI] public static readonly TweenMutator<Quaternion, Transform>
      rotation = (v, t, r) => { if (r) t.rotation *= v; else t.rotation = v; };

    #endregion

    #region RectTransform

    [PublicAPI] public static readonly TweenMutator<Vector2, RectTransform>
      anchoredPosition = (v, t, r) => { if (r) t.anchoredPosition += v; else t.anchoredPosition = v; };

    #endregion

    #region Graphic

    [PublicAPI] public static readonly TweenMutator<Color, Graphic>
      graphicColor = (c, g, r) => { if (r) g.color += c; else g.color = c; };

    [PublicAPI] public static readonly TweenMutator<float, Graphic> 
      graphicColorAlpha = (alpha, graphic, relative) => {
        var color = graphic.color;
        if (relative) color.a += alpha;
        else color.a = alpha;
        graphic.color = color;
      };

    #endregion

    #region Render Settings

    [PublicAPI] public static readonly TweenMutator<Color, Unit>
      globalFogColor = (v, _, r) => { if (r) RenderSettings.fogColor += v; else RenderSettings.fogColor = v; };

    [PublicAPI] public static readonly TweenMutator<float, Unit>
      globalFogDensity = (v, _, r) => { if (r) RenderSettings.fogDensity += v; else RenderSettings.fogDensity = v; };

    #endregion

    #region Light

    [PublicAPI] public static readonly TweenMutator<Color, Light>
      lightColor = (v, o, r) => { if (r) o.color += v; else o.color = v; };

    [PublicAPI] public static readonly TweenMutator<float, Light>
      lightIntensity = (v, o, r) => { if (r) o.intensity += v; else o.intensity = v; };

    #endregion

    #region Renderer

    [PublicAPI] public static readonly TweenMutator<Color, Renderer>
      rendererTint = (v, o, r) => {
        foreach (var material in o.materials) {
          if (r) material.color += v;
          else material.color = v;
        }
      };

    #endregion

    #region SpriteRenderer

    [PublicAPI] public static readonly TweenMutator<Color, SpriteRenderer>
      spriteRendererColor = (v, o, r) => { if (r) o.color += v; else o.color = v; };

    #endregion

    #region Text

    [PublicAPI] public static readonly TweenMutator<Color, Text>
      textColor = (v, o, r) => { if (r) o.color += v; else o.color = v; };

    #endregion
    
    [PublicAPI] public static readonly TweenMutator<float, Image>
      imageFillAmount = (v, o, r) => { if (r) o.fillAmount += v; else o.fillAmount = v; };

    [PublicAPI] public static readonly TweenMutator<Color, Shadow>
      shadowEffectColor = (color, shadow, r) => { if (r) shadow.effectColor += color; else shadow.effectColor = color; };

    [PublicAPI]
    public static TweenMutator<float, Transform> path(Vector3Path path) =>
      (percentage, transform, relative) => {
        var point = path.evaluate(percentage, constantSpeed: true);
        transform.localPosition = point;
      };
  }
}