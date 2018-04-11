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

      lineRenderer.positionCount = positions.Count;
      setVertexPositions();
    }

    void setVertexPositions() {
      var idx = 0;
      foreach (var pos in positions) {
        lineRenderer.SetPosition(idx, pos.position);
        idx++;
      }
    }
  }
}
