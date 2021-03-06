﻿#if UNITY_EDITOR
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public partial class LineRendererTrailDrawer {
    public void setSerializedData_onlyInEditor(float duration, float minVertexDistance, LineRenderer lineRenderer) {
      this.recordEditorChanges("Set time and distance");
      this.duration = duration;
      this.minVertexDistance = minVertexDistance;
      this.lineRenderer = lineRenderer;
    }
  }
}
#endif