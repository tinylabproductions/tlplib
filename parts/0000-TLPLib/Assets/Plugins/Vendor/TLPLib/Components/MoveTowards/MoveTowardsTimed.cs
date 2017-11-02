using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace Assets.Plugins.Vendor.TLPLib.Components.MoveTowards {
  public class MoveTowardsTimed : MoveTowardsBase, IMB_OnValidate {

#pragma warning disable 649
    [SerializeField] float time = 1;
#pragma warning restore 649

    protected override void run(float deltaTime, float elapsedTime) {
      t.position = Vector3.Lerp(
        t.position,
        target.position,
        deltaTime / (time - elapsedTime)
      );
    }

    public void OnValidate() {
      if (time < 0) time = 0;
    }
  }
}