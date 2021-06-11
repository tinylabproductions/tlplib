using System.Collections.Generic;
using Smooth.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.ui {
  public class SubdivideUIMesh : BaseMeshEffect {
    public uint subdivideCount = 1;

    public override void ModifyMesh(VertexHelper vh) {
      if (!IsActive()) return;
      if (subdivideCount == 0) return;

      var vertsOld = ListPool<UIVertex>.Instance.Borrow();
      var vertsUpdated = ListPool<UIVertex>.Instance.Borrow();
      vh.GetUIVertexStream(vertsOld);
      
      // Subdivide first time.
      subdivideOnce(vertsOld, vertsUpdated);
      // Repeat subdivisions.
      for (var i = 1; i < subdivideCount; i++) {
        var temp = vertsOld;
        vertsOld = vertsUpdated;
        vertsUpdated = temp;
        vertsUpdated.Clear();
        subdivideOnce(vertsOld, vertsUpdated);
      }
      
      vh.Clear();
      vh.AddUIVertexTriangleStream(vertsUpdated);
      ListPool<UIVertex>.Instance.Release(vertsOld);
      ListPool<UIVertex>.Instance.Release(vertsUpdated);
    }

    static void subdivideOnce(List<UIVertex> oldList, List<UIVertex> newList) {
      for (var i = 0; i < oldList.Count; i += 3) {
        var c1 = oldList[i];
        var c2 = oldList[i+1];
        var c3 = oldList[i+2];
        var m1 = lerp(c1, c2, .5f);
        var m2 = lerp(c2, c3, .5f);
        var m3 = lerp(c3, c1, .5f);
        newList.Add(c1); newList.Add(m1); newList.Add(m3);
        newList.Add(c2); newList.Add(m2); newList.Add(m1);
        newList.Add(c3); newList.Add(m3); newList.Add(m2);
        newList.Add(m1); newList.Add(m2); newList.Add(m3);
      }
    }
    
    static UIVertex lerp(UIVertex a, UIVertex b, float t) =>
      new UIVertex {
        position = Vector3.LerpUnclamped(a.position, b.position, t),
        normal = Vector3.LerpUnclamped(a.normal, b.normal, t),
        color = Color32.LerpUnclamped(a.color, b.color, t),
        tangent = Vector3.LerpUnclamped(a.tangent, b.tangent, t),
        uv0 = Vector3.LerpUnclamped(a.uv0, b.uv0, t),
        uv1 = Vector3.LerpUnclamped(a.uv1, b.uv1, t),
        uv2 = Vector3.LerpUnclamped(a.uv2, b.uv2, t),
        uv3 = Vector3.LerpUnclamped(a.uv3, b.uv3, t)
      };
  }
}
