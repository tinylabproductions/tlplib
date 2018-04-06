using System.Collections.Generic;
using UnityEngine;
using static com.tinylabproductions.TLPLib.Components.MeshGenerationHelpers;

namespace com.tinylabproductions.TLPLib.Components {
  public class LineMeshGenerator {
    const float LINES_PARALLEL_EPS = 0.2f;

    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<int> triangles = new List<int>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly List<Color> colors = new List<Color>();
    readonly float halfWidth;
    readonly Mesh m;
    readonly Gradient colorGradient;
    readonly AnimationCurve curve;
    readonly bool useWorldSpace;

    public LineMeshGenerator(
      float width, MeshFilter mf, Gradient colorGradient, AnimationCurve curve
    ) {
      halfWidth = width / 2;
      this.colorGradient = colorGradient;
      this.curve = curve;
      this.useWorldSpace = useWorldSpace;
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
      m.SetColors(colors);
      m.RecalculateBounds();
    }

    float getWidthForProgress(float progress) => curve.Evaluate(progress) * halfWidth;

    void fillVerticesAndUvs(int totalPositions, GetPosByIndex getPos) {
      var idx = 0;

      addDataForSegment(
        findCornersSimpleA(getPos(0), getPos(1), -getWidthForProgress(0f)),
        colorGradient.Evaluate(0), ref idx, totalPositions
      );
      for (var i = 1; i < totalPositions - 1; i++) {
        // totalPositions is always >= 2
        var progress = (float) i / (totalPositions - 1);
        var width = getWidthForProgress(progress);
        var color = colorGradient.Evaluate(progress);
        var cur = getPos(i);

        var prev = getPos(i - 1);
        var next = getPos(i + 1);
        if (Vector2.Angle(prev - cur, next - cur) < 90) {
          addDataForSegment(findCornersSimpleB(prev, cur, -width), color, ref idx, totalPositions);
          fillTriangle(idx);
          addDataForSegment(findCornersSimpleA(cur, next, -width), color, ref idx, totalPositions);
        }
        else {
          addDataForSegment(
            findCorners(prev, cur, next, -width, LINES_PARALLEL_EPS), color, ref idx, totalPositions
          );
          fillTriangle(idx);
        }
      }

      addDataForSegment(
        findCornersSimpleB(getPos(totalPositions - 2),  getPos(totalPositions - 1), -getWidthForProgress(1f)),
        colorGradient.Evaluate(1), ref idx, totalPositions
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

    void addDataForSegment(CornersData corners, Color color, ref int vertexIdx, int totalPositions) {
      // ReSharper disable once PossibleLossOfFraction
      var v = vertexIdx / 2 / (float) (totalPositions - 1);

      setOrAdd(vertices, corners.res1, vertexIdx);
      setOrAdd(uvs, new Vector2(v, 0), vertexIdx);
      setOrAdd(colors, color, vertexIdx);
      vertexIdx++;
      setOrAdd(vertices, corners.res2, vertexIdx);
      setOrAdd(uvs, new Vector2(v, 1), vertexIdx);
      setOrAdd(colors, color, vertexIdx);
      vertexIdx++;
    }

    static void setOrAdd<A>(IList<A> list, A a, int idx) {
      if (idx >= list.Count) list.Add(a);
      else list[idx] = a;
    }
  }
}
