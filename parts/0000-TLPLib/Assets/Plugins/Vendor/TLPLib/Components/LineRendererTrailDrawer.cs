using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [ExecuteInEditMode]
  public partial class LineRendererTrailDrawer : MonoBehaviour, IMB_Update {
    [Record]
    public partial struct PositionData {
      public readonly float time;
      public readonly Vector3 position;
    }

    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] LineRenderer lineRenderer;
    [SerializeField] float duration, minVertexDistance;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion
    
    readonly Deque<PositionData> positions = new Deque<PositionData>();
    
    bool shouldUpdate = true;
    IDisposable disposable = F.emptyDisposable;

    [PublicAPI]
    public void setRegularMode() {
      disposable.Dispose();
      shouldUpdate = true;
    }

    [PublicAPI]
    public void setForcedTrailMode(Vector3 size, Duration duration) {
      disposable.Dispose();
      disposable = new UnityCoroutine(this, routine(size, duration));
      shouldUpdate = false;
    }

    IEnumerator routine(Vector3 target, Duration duration) {
      var startPos = transform.position;
      var endPos = transform.position + target;
      lineRenderer.positionCount = 2;
      lineRenderer.SetPosition(0, startPos);
      lineRenderer.SetPosition(1, startPos);
      foreach (var p in new CoroutineInterval(duration)) {
        var current = Vector3.Lerp(startPos, endPos, p.value);
        lineRenderer.SetPosition(1, current);
        yield return null;
      }
    }

    public void Update() {
      if (!shouldUpdate) return;
      
      var currentTime = Time.time;
      var currentPos = transform.position;
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
