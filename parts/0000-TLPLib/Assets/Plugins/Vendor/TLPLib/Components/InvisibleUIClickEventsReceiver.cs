using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Components {
  /// <summary>
  /// Allows to receive UI click events even if we do not have a visible part for this UI object.
  /// </summary>
  public class InvisibleUIClickEventsReceiver : Graphic {
    // http://forum.unity3d.com/threads/recttransform-and-events.285740/
    // Do not generate mesh (do not call base).
    [Obsolete] protected override void OnFillVBO(List<UIVertex> vbo) {}

#if !UNITY_5_1 && !UNITY_5_0
    [Obsolete] protected override void OnPopulateMesh(Mesh m) => m.Clear();
#endif
  }
}
