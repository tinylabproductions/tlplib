using System.Collections.Generic;
using GenerationAttributes;
using UnityEngine;
using static com.tinylabproductions.TLPLib.Components.MeshGenerationHelpers;

namespace com.tinylabproductions.TLPLib.Components {
  public partial class LineMeshGenerator {

    [Record]
    public partial struct NodeData {
      public readonly Vector3 relativePosition;
      public readonly float distanceToPrevNode;
    }

    const float LINES_PARALLEL_EPS = 0.2f;

    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<int> triangles = new List<int>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly List<Color32> colors = new List<Color32>();
    readonly float halfWidth;
    readonly Mesh m;
    readonly Gradient colorGradient;
    readonly AnimationCurve curve;

    public LineMeshGenerator(
      float width, MeshFilter mf, Gradient colorGradient, AnimationCurve curve
    ) {
      halfWidth = width / 2;
      this.colorGradient = colorGradient;
      this.curve = curve;
      m = new Mesh();
      mf.sharedMesh = m;
    }

    public delegate NodeData GetNode(int nodeIdx);

    public void update(
      int totalPositions, float totalLineLength, GetNode getNode
    ) {
      if (totalPositions < 2) return;
      triangles.Clear();
      fillVerticesAndUvs(totalPositions, totalLineLength, getNode);
      m.SetVertices(vertices);
      m.SetTriangles(triangles, 0);
      m.SetUVs(0, uvs);
      m.SetColors(colors);
      m.RecalculateBounds();
    }

    float getWidthForProgress(float progress) => curve.Evaluate(progress) * halfWidth;

    void fillVerticesAndUvs(
      int totalPositions, float totalLineLength, GetNode getNode
    ) {
      var idx = 0;
      var lineLength = 0f;

      addDataForSegment(
        findCornersSimpleA(getNode(0).relativePosition, getNode(1).relativePosition, -getWidthForProgress(0f)),
        colorGradient.Evaluate(0), ref idx, 0f
      );
      for (var i = 1; i < totalPositions - 1; i++) {
        var curNode = getNode(i);

        lineLength += curNode.distanceToPrevNode;
        var progress = lineLength / totalLineLength;
        var width = getWidthForProgress(progress);
        var color = colorGradient.Evaluate(progress);

        var cur = curNode.relativePosition;
        var prev = getNode(i - 1).relativePosition;
        var next = getNode(i + 1).relativePosition;
        if (Vector2.Angle(prev - cur, next - cur) < 90) {
          addDataForSegment(findCornersSimpleB(prev, cur, -width), color, ref idx, progress);
          fillTriangle(idx);
          addDataForSegment(findCornersSimpleA(cur, next, -width), color, ref idx, progress);
        }
        else {
          addDataForSegment(
            findCorners(prev, cur, next, -width, LINES_PARALLEL_EPS), color, ref idx, progress
          );
          fillTriangle(idx);
        }
      }

      addDataForSegment(
        findCornersSimpleB(
          getNode(totalPositions - 2).relativePosition,
          getNode(totalPositions - 1).relativePosition, -getWidthForProgress(1f)
        ),
        colorGradient.Evaluate(1), ref idx, 1f
      );
      fillTriangle(idx);
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

    void addDataForSegment(CornersData corners, Color color, ref int vertexIdx, float progress) {
      setOrAdd(vertices, corners.res1, vertexIdx);
      setOrAdd(uvs, new Vector2(progress, 0), vertexIdx);
      setOrAdd(colors, color, vertexIdx);
      vertexIdx++;
      setOrAdd(vertices, corners.res2, vertexIdx);
      setOrAdd(uvs, new Vector2(progress, 1), vertexIdx);
      setOrAdd(colors, color, vertexIdx);
      vertexIdx++;
    }

    static void setOrAdd<A>(IList<A> list, A a, int idx) {
      if (idx >= list.Count) list.Add(a);
      else list[idx] = a;
    }
  }
}
