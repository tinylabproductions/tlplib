using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public static class Circle {
    [PublicAPI] public static Vector2[] unitPoints(int segments) {
      var points = new Vector2[segments];
      for (var i = 0; i < segments; i++) {
        var angle = i * Mathf.PI * 2 / segments;
        points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
      }

      return points;
    }
  }
}