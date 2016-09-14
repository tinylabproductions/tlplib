using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  class NextPostRenderBehaviour : MonoBehaviour {
    private int framesLeft;
    private Action action;

    public void init(Action action, int framesLeft) {
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
