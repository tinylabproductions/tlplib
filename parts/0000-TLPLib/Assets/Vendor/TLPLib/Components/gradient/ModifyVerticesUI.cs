using System.Collections.Generic;
using Smooth.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  public abstract class ModifyVerticesUI : BaseMeshEffect {
    public virtual void ModifyVertices(List<UIVertex> vertexList) {}

    public override void ModifyMesh(VertexHelper vh) {
      if (!IsActive()) return;
      var verts = ListPool<UIVertex>.Instance.Borrow();
      vh.GetUIVertexStream(verts);
      ModifyVertices(verts);  // calls the old ModifyVertices which was used on pre 5.2
      vh.Clear();
      vh.AddUIVertexTriangleStream(verts);
      ListPool<UIVertex>.Instance.Release(verts);
    }
  }
}