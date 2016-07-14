#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  public class JavaProxy : AndroidJavaProxy {
    public JavaProxy(string javaInterface) : base(javaInterface) {}
    public JavaProxy(AndroidJavaClass javaInterface) : base(javaInterface) {}

    /* May be called from Java side. */
    public string toString() { return ToString(); }
    public int hashCode() { return GetHashCode(); }
    public bool equals(object o) { return this == o; }
  }
}
#endif