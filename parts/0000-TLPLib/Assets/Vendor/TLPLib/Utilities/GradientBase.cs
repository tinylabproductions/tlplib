using System.Collections.Generic;
using Smooth.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.UI {
  public class GradientBase : BaseMeshEffect {

    public virtual void ModifyVertices(List<UIVertex> vertexList) {

    }
    public override void ModifyMesh(VertexHelper vh) {
      if (!this.IsActive()) return;
      var verts = ListPool<UIVertex>.Instance.Borrow();
      vh.GetUIVertexStream(verts);
      ModifyVertices(verts);  // calls the old ModifyVertices which was used on pre 5.2
      vh.AddUIVertexTriangleStream(verts);
    }
  }
}