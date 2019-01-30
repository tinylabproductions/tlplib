using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class DebugDraw {
    [Conditional("UNITY_EDITOR")]
    public static void circle(Vector3 pos, float radius, Color color, float duration, int segments = 20) {
      var current = pos + new Vector3(radius, 0);
      var segmentAngle = 2 * Mathf.PI / segments;
      for (var i = 1; i <= segments; i++) {
        var next = pos + radius * new Vector3(Mathf.Cos(segmentAngle * i), Mathf.Sin(segmentAngle * i));
        Debug.DrawLine(current, next, color, duration);
        current = next;
      }
    }

    [Conditional("UNITY_EDITOR")]
    public static void bounds2D(Bounds bounds, Color color, float duration = 0) {
      var c = bounds.center;
      var e = bounds.extents;
      var e2 = new Vector3(e.x, -e.y, e.z);
      Debug.DrawLine(c + e, c + e2, color, duration);
      Debug.DrawLine(c - e, c + e2, color, duration);
      Debug.DrawLine(c - e, c - e2, color, duration);
      Debug.DrawLine(c + e, c - e2, color, duration);
    }
  }
}