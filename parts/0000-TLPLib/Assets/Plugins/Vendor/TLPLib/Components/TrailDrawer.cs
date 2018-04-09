using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using Plugins.Vendor.TLPLib.Components;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  [RequireComponent(typeof(MeshFilter)), ExecuteInEditMode]
  public class TrailDrawer : TrailDrawerBase {

    #region Unity Serialized Fields

#pragma warning disable 649
// ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] float trailWidth = 3;
// ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion
    readonly LineMeshGenerator.GetPosByIndex getPosFn;
    readonly LazyVal<LineMeshGenerator> ropeGenerator;

    public TrailDrawer() {
      getPosFn = idx => positions[idx].position - transform.position;
      ropeGenerator = F.lazy(() => new LineMeshGenerator(trailWidth, gameObject.GetComponent<MeshFilter>()));
    }

    public override void Update() {
      base.Update();
      ropeGenerator.strict.update(positions.Count, getPosFn);
    }
  }
}
