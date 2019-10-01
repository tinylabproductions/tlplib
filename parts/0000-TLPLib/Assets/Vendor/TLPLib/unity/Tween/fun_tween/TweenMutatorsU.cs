using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Tween.fun_tween.path;
using JetBrains.Annotations;
using pzd.lib.functional;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  [PublicAPI] public static class TweenMutatorsU {
    static bool dead(Object o) {
      if (o) return false;
      else {
#if UNITY_EDITOR
        Log.d.error("Tween mutator target is dead!", o);
#endif
        return true;
      }
    }
    
    #region Transform

    [PublicAPI] public static readonly TweenMutator<Vector3, Transform>
      position = (v, t, r) => { if (dead(t)) return; if (r) t.position += v; else t.position = v; },
      localPosition = (v, t, r) => { if (dead(t)) return; if (r) t.localPosition += v; else t.localPosition = v; },
      localScale = (v, t, r) => { if (dead(t)) return; if (r) t.localScale += v; else t.localScale = v; },
      localEulerAngles = (v, t, r) => { if (dead(t)) return; if (r) t.localEulerAngles += v; else t.localEulerAngles = v; };
    
    [PublicAPI] public static readonly TweenMutator<Quaternion, Transform>
      rotation = (v, t, r) => { if (dead(t)) return; if (r) t.rotation *= v; else t.rotation = v; };

    #endregion

    #region RectTransform

    [PublicAPI] public static readonly TweenMutator<Vector2, RectTransform>
      anchoredPosition = (v, t, r) => { if (dead(t)) return; if (r) t.anchoredPosition += v; else t.anchoredPosition = v; };

    #endregion

    #region Graphic

    [PublicAPI] public static readonly TweenMutator<Color, Graphic>
      graphicColor = (c, g, r) => { if (dead(g)) return; if (r) g.color += c; else g.color = c; };

    [PublicAPI] public static readonly TweenMutator<float, Graphic> 
      graphicColorAlpha = (alpha, graphic, relative) => {
        if (dead(graphic)) return; 
        var color = graphic.color;
        if (relative) color.a += alpha;
        else color.a = alpha;
        graphic.color = color;
      };

    #endregion

    #region CanvasGroup

    [PublicAPI] public static readonly TweenMutator<float, CanvasGroup>
      canvasGroupAlpha = (v, cg, r) => { if (dead(cg)) return; if (r) cg.alpha += v; else cg.alpha = v; };

    #endregion

    #region Render Settings

    [PublicAPI] public static readonly TweenMutator<Color, Unit>
      globalFogColor = (v, _, r) => { if (r) RenderSettings.fogColor += v; else RenderSettings.fogColor = v; };

    [PublicAPI] public static readonly TweenMutator<float, Unit>
      globalFogDensity = (v, _, r) => { if (r) RenderSettings.fogDensity += v; else RenderSettings.fogDensity = v; };

    #endregion

    #region Light

    [PublicAPI] public static readonly TweenMutator<Color, Light>
      lightColor = (v, o, r) => { if (dead(o)) return; if (r) o.color += v; else o.color = v; };

    [PublicAPI] public static readonly TweenMutator<float, Light>
      lightIntensity = (v, o, r) => { if (dead(o)) return; if (r) o.intensity += v; else o.intensity = v; };

    #endregion

    #region Renderer

    [PublicAPI] public static readonly TweenMutator<Color, Renderer>
      rendererTint = (v, o, r) => {
        if (dead(o)) return; 
        foreach (var material in o.materials) {
          if (r) material.color += v;
          else material.color = v;
        }
      };

    #endregion

    #region SpriteRenderer

    [PublicAPI] public static readonly TweenMutator<Color, SpriteRenderer>
      spriteRendererColor = (v, o, r) => { if (dead(o)) return; if (r) o.color += v; else o.color = v; };

    #endregion

    #region Text

    [PublicAPI] public static readonly TweenMutator<Color, Text>
      textColor = (v, o, r) => { if (dead(o)) return; if (r) o.color += v; else o.color = v; };

    #endregion
    
    [PublicAPI] public static readonly TweenMutator<float, Image>
      imageFillAmount = (v, o, r) => { if (dead(o)) return; if (r) o.fillAmount += v; else o.fillAmount = v; };

    [PublicAPI] public static readonly TweenMutator<Color, Shadow>
      shadowEffectColor = (color, shadow, r) => {
        if (dead(shadow)) return;
        if (r) shadow.effectColor += color; else shadow.effectColor = color;
      };

    [PublicAPI]
    public static TweenMutator<float, Transform> path(Vector3Path path) =>
      (percentage, transform, relative) => {
        if (dead(transform)) return; 
        var point = path.evaluate(percentage, constantSpeed: true);
        transform.localPosition = point;
      };
    
    public static readonly TweenMutator<Color, TextMeshProUGUI>
      tmProColor = (v, o, r) => { if (dead(o)) return; if (r) o.color += v; else o.color = v; };
  }
}