using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class NextPostRenderBehaviour : MonoBehaviour {
    private int framesLeft;
    private Act action;

    public void init(Act action, int framesLeft) {
      this.action = action;
      this.framesLeft = framesLeft;
    }

    internal void OnPostRender() {
      if (framesLeft == 1) {
        action();
        Destroy(this);
      }
      else
        framesLeft--;
    }
  }
}
