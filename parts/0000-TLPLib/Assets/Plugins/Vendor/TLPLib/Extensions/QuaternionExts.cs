using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class QuaternionExts {
    [PublicAPI] public static float toRotation2D(this Quaternion q) => q.eulerAngles.z * Mathf.Deg2Rad;
  }
}