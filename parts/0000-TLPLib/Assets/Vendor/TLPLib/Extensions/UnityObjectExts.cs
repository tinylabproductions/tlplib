using com.tinylabproductions.TLPLib.Filesystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Vendor.TLPLib.Extensions {
  public static class UnityObjectExts {
#if UNITY_EDITOR
    public static PathStr path(this Object obj) {
      return new PathStr(AssetDatabase.GetAssetPath(obj));
    }
#endif
  }
}
