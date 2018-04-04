using UnityEngine;
using static com.tinylabproductions.TLPLib.Components.MeshGenerationHelpers;

namespace Assets.Code.Utils {
  public class RopeMeshGenerator {
    readonly int subSegments, totalSegments;
    readonly float width;
    readonly Transform[] transforms;
    readonly MeshFilter mf;

    Vector3[] vertices;
    Mesh m;

    public RopeMeshGenerator(int subSegments, float width, Transform[] transforms, MeshFilter mf) {
      this.subSegments = subSegments;
      this.width = width;
      this.transforms = transforms;
      this.mf = mf;

      var linePoints = transforms.Length;
      totalSegments = (linePoints - 1) * subSegments + 1;

      createMesh();
    }

    public void update() {
      fillVertices(vertices);
      m.vertices = vertices;
      m.RecalculateBounds();
    }

    void createMesh() {
      m = new Mesh();
      var nodeCount = totalSegments * 2;
      var triangleCount = nodeCount - 2;
      vertices = new Vector3[nodeCount];
      var triangles = new int[triangleCount * 3];
      var uv = new Vector2[nodeCount];
      fillVertices(vertices);
      fillTriangles(triangles);
      fillUV(uv);

      m.vertices = vertices;
      m.triangles = triangles;
      m.uv = uv;
      mf.sharedMesh = m;
    }

    void fillUV(Vector2[] uv) {
      var hLen = uv.Length / 2;
      for (var i = 0; i < hLen; i++) {
        var v = hLen / (float) i;
        uv[i*2] = new Vector2(0, v);
        uv[i*2+1] = new Vector2(1, v);
      }
    }

    void fillTriangles(int[] triangles) {
      int c = 0;
      for (var i = 1; i < totalSegments; i++) {
        /*
        -2  |  -1
        ----+----
        -4  |  -3
        */
        var vc = (i+1) * 2;

        triangles[c++] = vc - 4;
        triangles[c++] = vc - 2;
        triangles[c++] = vc - 3;

        triangles[c++] = vc - 1;
        triangles[c++] = vc - 3;
        triangles[c++] = vc - 2;
      }
    }

    Vector2 getPoint(int i) {
      var n = transforms.Length;

      var perc = i % subSegments / (float) subSegments;
      i /= subSegments;

      var a1 = getVect2(Mathf.Clamp(i - 1, 0, n - 1));
      var a2 = getVect2(i);
      var a3 = getVect2(Mathf.Clamp(i + 1, 0, n - 1));
      var a4 = getVect2(Mathf.Clamp(i + 2, 0, n - 1));

      return new Vector2(
        Hermite(a1.x, a2.x, a3.x, a4.x, perc, 0, 0),
        Hermite(a1.y, a2.y, a3.y, a4.y, perc, 0, 0)
      );
    }

    Vector2 getVect2(int i) {
      return transforms[i].localPosition;
    }

    void fillVertices(Vector3[] vertices) {
      var leftWidth = width / 2;
      for (var i = 0; i < totalSegments; i++) {
        var cur = getPoint(i);
        if (i == 0) {
          var next = getPoint(i + 1);
          var corners = findCornersSimpleA(cur, next, -leftWidth);
          vertices[i * 2] = corners.res1;
          vertices[i * 2 + 1] = corners.res2;

        }
        else {
          var prev = getPoint(i - 1);
          var corners = findCornersSimpleB(prev, cur, -leftWidth);
          vertices[i * 2] = corners.res1;
          vertices[i * 2 + 1] = corners.res2;

        }
      }
    }
  }
}
