using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class LayerMaskExts {
    [PublicAPI] public static bool isInLayerMask(this LayerMask mask, int layer) => (mask.value & (1 << layer)) > 0;
    [PublicAPI] public static bool isInLayerMask(this LayerMask mask, GameObject go) => mask.isInLayerMask(go.layer);
    [PublicAPI] public static bool isInLayerMask(this LayerMask mask, Collider2D col) => mask.isInLayerMask(col.gameObject);
  }
}