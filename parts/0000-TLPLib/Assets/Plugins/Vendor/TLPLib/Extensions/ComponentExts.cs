using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ComponentExts {
    public static A clone<A>(
      this A self, Vector3? position=null, Quaternion? rotation=null, Transform parent=null, 
      int? siblingIndex=null, bool? setActive=null
    ) where A : Component {
      // Object is instantiated directly into target hierarchy, this way it does not allocate new transform buffer,
      // because we never create new root transform. 
      var cloned = parent != null ? Object.Instantiate(self, parent, false) : Object.Instantiate(self);
      if (position != null) cloned.transform.position = (Vector3) position;
      if (rotation != null) cloned.transform.rotation = (Quaternion) rotation;
      if (siblingIndex != null) cloned.transform.SetSiblingIndex((int) siblingIndex);
      if (setActive != null) cloned.gameObject.SetActive((bool) setActive);
      return cloned;
    }
  }
}
