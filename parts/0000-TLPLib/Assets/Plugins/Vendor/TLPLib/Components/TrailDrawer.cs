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

    readonly LineMeshGenerator.GetPosByIndex getPosFn;
    readonly LazyVal<LineMeshGenerator> lineMeshGenerator;

    public TrailDrawer() {
      getPosFn = idx => positions[idx].position - getTransformPosition();
      lineMeshGenerator = F.lazy(() => new LineMeshGenerator(
        trailWidth, gameObject.GetComponent<MeshFilter>(), color, widthMultiplierCurve)
      );
    }

    public override void LateUpdate() {
      base.LateUpdate();
      lineMeshGenerator.strict.update(positions.Count, getPosFn);
      // Trail should not be rotated with the parent
      transform.rotation = Quaternion.identity;
    }
  }
}
