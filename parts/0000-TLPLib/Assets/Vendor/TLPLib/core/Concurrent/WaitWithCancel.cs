using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class WaitWithCancel : CustomYieldInstruction {
    readonly Func<bool> continueWaiting;
    readonly bool unscaledTime;
    readonly float waitUntil;

    public WaitWithCancel(float seconds, Func<bool> continueWaiting, bool unscaledTime = false) {
      this.continueWaiting = continueWaiting;
      this.unscaledTime = unscaledTime;
      waitUntil = getTime() + seconds;
    }

    float getTime() => unscaledTime ? Time.unscaledTime : Time.time;

    public override bool keepWaiting { get {
      if (!continueWaiting()) return false;
      return getTime() < waitUntil;
    } }
  }
}