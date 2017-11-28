using System;
using com.tinylabproductions.TLPLib.Concurrent;
using UnityEngine;

namespace Assets.Plugins.Vendor.TLPLib.Utilities {
  public class LoopingAction {
    bool active;
    readonly MonoBehaviour monoBehaviour;

    public LoopingAction(MonoBehaviour monoBehaviour) {
      this.monoBehaviour = monoBehaviour;
    }

    public void repeatWithCooldown(float cooldown, Action action) {
      active = true;
      ASync.EveryXSeconds(cooldown, monoBehaviour, () => {
        if (active)
          action();
        return active;
      });
    }

    public void stop() => active = false;
  }
}