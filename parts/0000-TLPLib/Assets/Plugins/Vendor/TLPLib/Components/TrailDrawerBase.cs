using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Components {
  [ExecuteInEditMode]
  public abstract partial class TrailDrawerBase : MonoBehaviour, IMB_LateUpdate {
    [Record]
    protected partial struct PositionData {
      public readonly float time;
      public Vector3 position;
    }

    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] protected float duration, minVertexDistance;
    [SerializeField] protected Vector3 forcedLocalSpeed, forcedWorldSpeed;
    [SerializeField] protected bool useWorldSpace = true;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    protected readonly Deque<PositionData> positions = new Deque<PositionData>();

    [PublicAPI]
    public void setForcedLocalSpeed(Vector3 speed) => forcedLocalSpeed = speed;

    [PublicAPI]
    public void setForcedWorldSpeed(Vector3 speed) => forcedWorldSpeed = speed;

    public virtual void LateUpdate() {
      var currentTime = Time.time;
      var deltaTime = Time.deltaTime;
      var currentPos = getTransformPosition();

      if (forcedLocalSpeed != Vector3.zero || forcedWorldSpeed != Vector3.zero) {
        var worldSpeed = transform.TransformDirection(forcedLocalSpeed) + forcedWorldSpeed;
        for (var i = 0; i < positions.Count; i++) {
          positions.GetRef(i).position += worldSpeed * deltaTime;
        }
      }

      while (positions.Count > 0 && positions[positions.Count - 1].time + duration < currentTime) {
        positions.RemoveBack();
      }

      if (shouldAddPoint(currentPos)) positions.AddFront(new PositionData(currentTime, currentPos));
    }

    bool shouldAddPoint(Vector3 currentPos) =>
      positions.Count == 0
      || Vector3.Distance(positions[0].position, currentPos) >= minVertexDistance;

    protected Vector3 getTransformPosition() => useWorldSpace ? transform.position : transform.localPosition;
  }
}