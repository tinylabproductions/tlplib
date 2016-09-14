using System;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class ASyncFixedUpdateHelperBehaviour : MonoBehaviour, IMB_FixedUpdate {
    float timeLeft;
    Action act;

    public void init(float timeLeft, Action act) {
      this.timeLeft = timeLeft;
      this.act = act;
    }

    public void FixedUpdate() {
      if (act == null) {
        // Fixed update gets called before init
        // Don't ask me why
        return;
      }
      timeLeft -= Time.fixedDeltaTime;
      if (timeLeft <= 1e-5) {
        act();
        Destroy(this);
      }
    }
  }
}
