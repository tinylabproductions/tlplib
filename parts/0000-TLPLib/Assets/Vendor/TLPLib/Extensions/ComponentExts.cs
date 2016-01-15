using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ComponentExts {
    public static A clone<A>(
      this A self, Vector3? position=null, Quaternion? rotation=null, 
      Transform parent=null
    ) where A : Component {
      var cloned = Object.Instantiate(self.gameObject);
      if (position != null) cloned.transform.position = (Vector3) position;
      if (rotation != null) cloned.transform.rotation = (Quaternion) rotation;
      if (parent != null) cloned.transform.parent = parent;
      return cloned.GetComponent<A>();
    }
  }
}
