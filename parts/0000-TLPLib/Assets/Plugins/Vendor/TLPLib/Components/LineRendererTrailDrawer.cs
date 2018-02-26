using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [ExecuteInEditMode]
  public partial class LineRendererTrailDrawer : MonoBehaviour, IMB_Update {
    
    public struct ForcedTrailData {
      public readonly float startTime, minVertexTime;
      public readonly Vector3 size;

      public ForcedTrailData(float startTime, Vector3 size, float minVertexTime) {
        this.startTime = startTime;
        this.size = size;
        this.minVertexTime = minVertexTime;
      }
    }
    
    enum Mode : byte { Regular, ForcedTrail } 
    
    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField, NotNull] LineRenderer lineRenderer;
    [SerializeField] float duration, minVertexDistance;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion
    
    readonly Queue<Tpl<float, Vector3>> positions = new Queue<Tpl<float, Vector3>>();

    Mode mode = Mode.Regular;
    ForcedTrailData forcedData;
    
    public void setForceTrailMode(Vector3 size) {
      forcedData = new ForcedTrailData(Time.time, size, duration / (size.magnitude / minVertexDistance));
      positions.Clear();
      positions.Enqueue(F.t(forcedData.startTime, transform.position));
    }

    public void setRegularMode() => mode = Mode.Regular;

    public void Update() {
      var currentTime = Time.time;
      var currentPos = transform.position;

      if (mode == Mode.ForcedTrail) {
        if (
          positions.Count == 0 || 
           (currentTime - positions.Last()._1 >= forcedData.minVertexTime && positions.Last()._1 < forcedData.startTime + duration ) 
        ) {
          var pos = Vector3.Lerp(currentPos, currentPos + forcedData.size, (currentTime - forcedData.startTime) / duration);
          positions.Enqueue(F.t(currentTime, pos));
        }
      }
      else {
        if (positions.Count == 0 || Vector3.Distance(positions.Last()._2, currentPos) >= minVertexDistance) {
          positions.Enqueue(F.t(currentTime, currentPos));
        }

        var queueing = true;
        while (queueing && positions.Count > 0) {
          var tpl = positions.Peek();
          if (tpl._1 + duration < currentTime) {
            positions.Dequeue();
          }
          else queueing = false;
        }
      }
      lineRenderer.positionCount = positions.Count;
      setPositions();
    }

    void setPositions() {
      var idx = 0;
      foreach (var pos in positions) {
        lineRenderer.SetPosition(idx, pos._2);
        idx++;
      }
    }
  }
}