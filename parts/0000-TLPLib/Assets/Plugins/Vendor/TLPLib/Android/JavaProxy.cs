#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  public class JavaProxy : AndroidJavaProxy {
    public JavaProxy(string javaInterface) : base(javaInterface) {}
    public JavaProxy(AndroidJavaClass javaInterface) : base(javaInterface) {}

    /* May be called from Java side. */
    public string toString() => ToString();
    public int hashCode() => GetHashCode();
    public bool equals(object o) => this == o;
  }
}
#endif