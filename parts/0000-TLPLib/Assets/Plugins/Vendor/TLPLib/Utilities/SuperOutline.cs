using System.Collections.Generic;
using Smooth.Pools;

namespace UnityEngine.UI {
  public class SuperOutline : Shadow {
    protected SuperOutline() { }

    public int count = 8;

#if UNITY_5_0 || UNITY_5_1
    public override void ModifyVertices(List<UIVertex> verts) {
      if (!IsActive())
        return;

      count = Mathf.Clamp(count, 1, 100);

      var diff = Mathf.PI * 2 / count;
      var start = 0;
      var end = verts.Count;
      for (var i = 0; i < count; i++) {
        var angle = diff * i;
        ApplyShadow(verts, effectColor, start, verts.Count, Mathf.Cos(angle) * effectDistance.x, Mathf.Sin(angle) * effectDistance.y);
        start = end;
        end = verts.Count;
      }
    }
#else
    public override void ModifyMesh(VertexHelper vh) {
      if (!IsActive())
        return;

      var verts = ListPool<UIVertex>.Instance.Borrow();
      vh.GetUIVertexStream(verts);

      count = Mathf.Clamp(count, 1, 100);

      var diff = Mathf.PI * 2 / count;
      var start = 0;
      var end = verts.Count;
      for (var i = 0; i < count; i++) {
        var angle = diff * i;
        ApplyShadow(verts, effectColor, start, verts.Count, Mathf.Cos(angle) * effectDistance.x, Mathf.Sin(angle) * effectDistance.y);
        start = end;
        end = verts.Count;
      }
      vh.Clear();
      vh.AddUIVertexTriangleStream(verts);
      ListPool<UIVertex>.Instance.Release(verts);
    }
#endif
  }
}