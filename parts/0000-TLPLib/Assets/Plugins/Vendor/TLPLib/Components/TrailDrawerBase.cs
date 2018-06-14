using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Components {
  [ExecuteInEditMode]
  public abstract partial class TrailDrawerBase : MonoBehaviour, IMB_LateUpdate {
    [Record]
    protected partial struct NodeData {
      public readonly float time;
      public Vector3 position;

      /*
      ** Distance to a previous node (by index) in a node queue.
      ** e.g. nodes[0] is previous of nodes[1]
      ** 0 if it's the first element in the queue.
       */
      public float distanceToPrevNode;
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

    protected readonly Deque<NodeData> nodes = new Deque<NodeData>();

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
        var offset = worldSpeed * deltaTime;
        for (var i = 0; i < nodes.Count; i++) {
          nodes.GetRef(i).position += offset;
        }
      }

      while (nodes.Count >= 2 && nodes[nodes.Count - 1].time + duration < currentTime) {
        var prev = nodes[nodes.Count - 2];
        var last = nodes[nodes.Count - 1];

        if (prev.time + duration < currentTime) {
          nodes.RemoveBack();
        }
        else {
          var newPos = Vector3.Lerp(
            last.position, prev.position, Mathf.InverseLerp(last.time, prev.time, currentTime - duration)
          );

          var distToPrevNode = Vector3.Distance(newPos, nodes[nodes.Count - 2].position);

          // We remove if distToPrevNode is ~0 because otherwise visual bugs might occur
          if (Mathf.Approximately(distToPrevNode, 0)) nodes.RemoveBack();
          else {
            nodes[nodes.Count - 1] = new NodeData(
              time: currentTime - duration,
              position: newPos,
              distanceToPrevNode: distToPrevNode
            );
          }
          break;
        }
      }

      if (shouldAddPoint(currentPos)) {
        nodes.AddFront(new NodeData(currentTime, currentPos, 0f));
        if (nodes.Count > 1) {
          nodes.GetRef(1).distanceToPrevNode = Vector3.Distance(nodes[1].position, nodes[0].position);
        }
      }
    }

    bool shouldAddPoint(Vector3 currentPos) =>
      nodes.Count == 0
      || Vector3.Distance(nodes[0].position, currentPos) >= minVertexDistance;

    protected Vector3 getTransformPosition() => useWorldSpace ? transform.position : transform.localPosition;
  }
}