using JetBrains.Annotations;
using Plugins.Vendor.TLPLib.Components;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [ExecuteInEditMode]
  public partial class LineRendererTrailDrawer : TrailDrawerBase {
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] LineRenderer lineRenderer;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion
    
    public override void LateUpdate() {
      base.LateUpdate();

      lineRenderer.positionCount = nodes.Count;
      setVertexPositions();
    }

    void setVertexPositions() {
      var idx = 0;
      foreach (var node in nodes) {
        lineRenderer.SetPosition(idx, node.position);
        idx++;
      }
    }
  }
}
