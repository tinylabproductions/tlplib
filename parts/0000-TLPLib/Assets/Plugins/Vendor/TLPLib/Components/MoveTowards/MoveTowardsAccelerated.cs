using UnityEngine;

namespace Assets.Plugins.Vendor.TLPLib.Components.MoveTowards {
  public class MoveTowardsAccelerated : MoveTowardsBase {

#pragma warning disable 649
    [SerializeField] float initialSpeed, acceleration;
#pragma warning restore 649

    protected override void run(float deltaTime, float elapsedTime) {
      var speed = initialSpeed + acceleration * elapsedTime;
      var distance = speed * deltaTime;
      t.position =
        Vector3.MoveTowards(
          current: t.position,
          target: target.position,
          maxDistanceDelta: distance
        );
    }
  }
}