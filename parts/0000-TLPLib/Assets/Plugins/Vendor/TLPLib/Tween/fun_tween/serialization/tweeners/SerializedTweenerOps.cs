using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  public static class SerializedTweenerOps {
    public delegate A Add<A>(A a1, A a2);
    public delegate ValueType Extract<out ValueType, in TargetType>(TargetType t);

    public static class Add {
      public static readonly Add<Color> color = (a1, a2) => a1 + a2;
      public static readonly Add<float> float_ = (a1, a2) => a1 + a2;
      public static readonly Add<Vector2> vector2 = (a1, a2) => a1 + a2;
      public static readonly Add<Vector3> vector3 = (a1, a2) => a1 + a2;
      public static readonly Add<Quaternion> quaternion = (a1, a2) => a1 * a2;
    }

    public static class Extract {
      public static readonly Extract<Color, Unit> globalFogColor = _ => RenderSettings.fogColor;
      public static readonly Extract<float, Unit> globalFogDensity = _ => RenderSettings.fogDensity;
      public static readonly Extract<Color, Graphic> graphicColor = _ => _.color;
      public static readonly Extract<float, Graphic> graphicColorAlpha = _ => _.color.a;
      public static readonly Extract<float, Image> imageFillAmount = _ => _.fillAmount;
      public static readonly Extract<Color, Light> lightColor = _ => _.color;
      public static readonly Extract<float, Light> lightIntensity = _ => _.intensity;
      public static readonly Extract<Vector2, RectTransform> anchoredPosition = _ => _.anchoredPosition;
      public static readonly Extract<Color, Renderer> rendererTint = _ => _.material.color;
      public static readonly Extract<Color, Shadow> shadowEffectColor = _ => _.effectColor;
      public static readonly Extract<Color, SpriteRenderer> spriteRendererColor = _ => _.color;
      public static readonly Extract<Color, Text> textColor = _ => _.color;
      public static readonly Extract<Vector3, Transform> 
        localEulerAngles = _ => _.localEulerAngles,
        localPosition = _ => _.localPosition,
        localScale = _ => _.localScale,
        position = _ => _.position;
      public static readonly Extract<Quaternion, Transform> rotation = _ => _.rotation;
    }
  }
}