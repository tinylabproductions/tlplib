using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [ExecuteInEditMode]
  public partial class LineRendererTrailDrawer : MonoBehaviour, IMB_Update {
    [Record]
    partial struct PositionData {
      public readonly float time;
      public Vector3 position;
    }

    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] LineRenderer lineRenderer;
    [SerializeField] float duration, minVertexDistance;
    [SerializeField] Vector3 forcedLocalSpeed, forcedWorldSpeed;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion
    
    readonly Deque<PositionData> positions = new Deque<PositionData>();

    [PublicAPI]
    public void setForcedLocalSpeed(Vector3 speed) => forcedLocalSpeed = speed;

    [PublicAPI]
    public void setForcedWorldSpeed(Vector3 speed) => forcedWorldSpeed = speed;

    public void Update() {
      var currentTime = Time.time;
      var currentPos = transform.position;

      if (forcedLocalSpeed != Vector3.zero || forcedWorldSpeed != Vector3.zero ) {
        var worldSpeed = transform.TransformDirection(forcedLocalSpeed) + forcedWorldSpeed;
        for (var i = 0; i < positions.Count; i++) {
          positions.GetRef(i).position += worldSpeed * Time.deltaTime;
        }
      }

      if (shouldAddPoint(currentPos)) positions.AddFront(new PositionData(currentTime, currentPos));
      var queueing = true;
      while (queueing && positions.Count > 0) {
        var position = positions[positions.Count - 1];
        if (position.time + duration < currentTime) {
          positions.RemoveBack();
        }
        else queueing = false;
      }

      lineRenderer.positionCount = positions.Count;
      setVertexPositions();
    }

    bool shouldAddPoint(Vector3 currentPos) =>
      positions.Count == 0
      || Vector3.Distance(positions[0].position, currentPos) >= minVertexDistance;

    void setVertexPositions() {
      var idx = 0;
      foreach (var pos in positions) {
        lineRenderer.SetPosition(idx, pos.position);
        idx++;
      }
    }
  }
}
