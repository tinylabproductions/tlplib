using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using Plugins.Vendor.TLPLib.Components;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [
    RequireComponent(typeof(MeshFilter)),
    RequireComponent(typeof(MeshRenderer)),
    ExecuteInEditMode
  ]
  public class TrailDrawer : TrailDrawerBase {

    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] float trailWidth = 3;
    [SerializeField, NotNull] Gradient color = new Gradient();
    [SerializeField, NotNull] AnimationCurve widthMultiplierCurve = AnimationCurve.Linear(0, 1, 1, 1);
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    readonly LineMeshGenerator.GetNode getNode;
    readonly LazyVal<LineMeshGenerator> lineMeshGenerator;

    public TrailDrawer() {
      getNode = idx => new LineMeshGenerator.NodeData(
        relativePosition: nodes[idx].position - getTransformPosition(),
        distanceToPrevNode: nodes[idx].distanceToPrevNode
      );
      lineMeshGenerator = F.lazy(() => new LineMeshGenerator(
        trailWidth, gameObject.GetComponent<MeshFilter>(), color, widthMultiplierCurve)
      );
    }

    public override void LateUpdate() {
      base.LateUpdate();
      lineMeshGenerator.strict.update(
        totalPositions: nodes.Count,
        totalLineLength: cakculateTotalLength(),
        getNode: getNode
      );
      // Trail should not be rotated with the parent
      transform.rotation = Quaternion.identity;
    }

    float cakculateTotalLength() {
      var sum = 0f;
      for (var i = 0; i < nodes.Count; i++) {
        sum += nodes[i].distanceToPrevNode;

      }
      return sum;
    }
  }
}
