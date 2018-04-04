using System.Collections.Generic;
using UnityEngine;
using static com.tinylabproductions.TLPLib.Components.MeshGenerationHelpers;

namespace com.tinylabproductions.TLPLib.Components {
  public class LineMeshGenerator {
    const float LINES_PARALLEL_EPS = 0.2f;

    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<int> triangles = new List<int>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly float width;
    readonly Mesh m;

    public LineMeshGenerator(float width, MeshFilter mf) {
      this.width = width;
      m = new Mesh();
      mf.sharedMesh = m;
    }

    public delegate Vector3 GetPosByIndex(int idx);

    public void update(int totalPositions, GetPosByIndex getPos) {
      if (totalPositions < 2) return;
      triangles.Clear();
      fillVerticesAndUvs(totalPositions, getPos);
      m.SetVertices(vertices);
      m.SetTriangles(triangles, 0);
      m.SetUVs(0, uvs);
      m.RecalculateBounds();
    }

    void fillVerticesAndUvs(int totalPositions, GetPosByIndex getPos) {
      var leftWidth = width / 2;
      var idx = 0;

      addVertsAndUvsForSegment(findCornersSimpleA(getPos(0), getPos(1), -leftWidth), ref idx, totalPositions);
      for (var i = 1; i < totalPositions - 1; i++) {
        var cur = getPos(i);

        var prev = getPos(i - 1);
        var next = getPos(i + 1);
        if (Vector2.Angle(prev - cur, next - cur) < 90) {
          addVertsAndUvsForSegment(findCornersSimpleB(prev, cur, -leftWidth), ref idx, totalPositions);
          fillTriangle(idx);
          addVertsAndUvsForSegment(findCornersSimpleA(cur, next, -leftWidth), ref idx, totalPositions);
        }
        else {
          addVertsAndUvsForSegment(
            findCorners(prev, cur, next, -leftWidth, LINES_PARALLEL_EPS), ref idx, totalPositions
          );
          fillTriangle(idx);
        }
      }

      addVertsAndUvsForSegment(
        findCornersSimpleB(getPos(totalPositions - 2),  getPos(totalPositions - 1), -leftWidth), ref idx, totalPositions
      );
    }

    void fillTriangle(int idx) {
      /*
       -2  |  -1
       ----+----
       -4  |  -3
       */
      triangles.Add(idx - 4);
      triangles.Add(idx - 2);
      triangles.Add(idx - 3);

      triangles.Add(idx - 1);
      triangles.Add(idx - 3);
      triangles.Add(idx - 2);
    }

    void addVertsAndUvsForSegment(CornersData corners, ref int vertexIdx, int totalPositions) {
      // ReSharper disable once PossibleLossOfFraction
      var v = vertexIdx / 2 / (float) (totalPositions - 1);

      setOrAdd(vertices, corners.res1, vertexIdx);
      setOrAdd(uvs, new Vector2(v, 0), vertexIdx);
      vertexIdx++;
      setOrAdd(vertices, corners.res2, vertexIdx);
      setOrAdd(uvs, new Vector2(v, 1), vertexIdx);
      vertexIdx++;
    }

    static void setOrAdd<A>(IList<A> list, A a, int idx) {
      if (idx >= list.Count) list.Add(a);
      else list[idx] = a;
    }
  }
}
