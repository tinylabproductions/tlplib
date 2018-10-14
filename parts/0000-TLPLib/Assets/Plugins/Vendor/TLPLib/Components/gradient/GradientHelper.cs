using System;
using UnityEngine;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public static class GradientHelper {
    public enum GradientType { Vertical, Horizontal }

    public static void modifyVertices(
      List<UIVertex> vertexList, Fn<Color32, float, Color32> f, GradientType type, bool useGraphicAlpha
    ) {
      switch (type) {
        case GradientType.Vertical:
          modifyVertices(vertexList, f, v => v.y, useGraphicAlpha);
          break;
        case GradientType.Horizontal:
          modifyVertices(vertexList, f, v => v.x, useGraphicAlpha);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }
    }

    static void modifyVertices(
      List<UIVertex> vertexList, Fn<Color32, float, Color32> f, Fn<Vector3, float> getAxisFn, bool useGraphicAlpha
    ) {
      var count = vertexList.Count;
      if (count == 0) return;
      var min = getAxisFn(vertexList[0].position);
      var max = getAxisFn(vertexList[0].position);

      for (var i = 1; i < count; i++) {
        var current = getAxisFn(vertexList[i].position);
        if (current > max) {
          max = current;
        }
        else if (current < min) {
          min = current;
        }
      }

      var uiElementHeight = max - min;

      for (var i = 0; i < count; i++) {
        var uiVertex = vertexList[i];
        var color = f(uiVertex.color, (getAxisFn(uiVertex.position) - min) / uiElementHeight);
        if (useGraphicAlpha) {
          // Taken from UnityEngine.UI.Shadow.cs
          color.a = (byte) (color.a * vertexList[i].color.a / byte.MaxValue);
        }
        uiVertex.color = color;
        vertexList[i] = uiVertex;
      }
    }
  }
}