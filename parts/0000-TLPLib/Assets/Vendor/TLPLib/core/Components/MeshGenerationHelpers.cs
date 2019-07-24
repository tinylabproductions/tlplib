using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public static class MeshGenerationHelpers {
    public static float crossProduct(Vector2 v, Vector2 w) => v.x * w.y - v.y * w.x;

    // Copied from Ferr2D
    public static float Hermite(
      float v1, float v2, float v3, float v4, float aPercentage, float aTension, float aBias
    ) {
      var mu2 = aPercentage * aPercentage;
      var mu3 = mu2 * aPercentage;
      var m0 = (v2 - v1) * (1 + aBias) * (1 - aTension) / 2 + (v3 - v2) * (1 - aBias) * (1 - aTension) / 2;
      var m1 = (v3 - v2) * (1 + aBias) * (1 - aTension) / 2 + (v4 - v3) * (1 - aBias) * (1 - aTension) / 2;
      var a0 = 2 * mu3 - 3 * mu2 + 1;
      var a1 = mu3 - 2 * mu2 + aPercentage;
      var a2 = mu3 - mu2;
      var a3 = -2 * mu3 + 3 * mu2;

      return (a0 * v2 + a1 * m0 + a2 * m1 + a3 * v3);
    }

    public static CornersData findCorners(Vector2 prev, Vector2 cur, Vector2 next, float dist, float linesParallelEps) {
      //LOVE game engine Polyline.cpp

      var t = prev - cur;
      var tLen = t.magnitude;
      var nt = new Vector2(-t.y, t.x) * (dist / tLen);

      var s = cur - next;
      var sLen = s.magnitude;
      var ns = new Vector2(-s.y, s.x) * (dist / sLen);

/*      var joinNormal = (nt + ns).normalized;
      var cosAngle = Vector2.Dot(joinNormal, ns);
      var mitter = joinNormal / cosAngle * dist;

      res1 = cur + mitter;
      res2 = cur - mitter;
      return;*/

      var det = crossProduct(s, t);

      if (Mathf.Abs(det) / (sLen * tLen) < linesParallelEps) {
        // lines parallel, compute as u1 = q + ns * w/2, u2 = q - ns * w/2
        return new CornersData(cur + ns, cur - ns);
      }

      // cramers rule
      var lambda = crossProduct(nt - ns, t) / det;
      var d = ns + s * lambda;
      return new CornersData(cur + d, cur - d);
    }

    public static CornersData findCornersSimpleA(Vector2 cur, Vector2 next, float dist) {
      var t = cur - next;
      var nt = new Vector2(-t.y, t.x).normalized * dist;
      return new CornersData(cur + nt, cur - nt);
    }

    public static CornersData findCornersSimpleB(Vector2 prev, Vector2 cur, float dist) {
      var t = prev - cur;
      var nt = new Vector2(-t.y, t.x).normalized * dist;
      return new CornersData(cur + nt, cur - nt);
    }
  }

  public class CornersData {
    public readonly Vector3 res1, res2;
    public CornersData(Vector3 res1, Vector3 res2) {
      this.res1 = res1;
      this.res2 = res2;
    }
  }
}