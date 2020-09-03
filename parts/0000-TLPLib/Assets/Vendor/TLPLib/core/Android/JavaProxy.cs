#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Concurrent;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  public class JavaProxy : AndroidJavaProxy {
    public JavaProxy(string javaInterface) : base(javaInterface) {}
    public JavaProxy(AndroidJavaClass javaInterface) : base(javaInterface) {}

    /* May be called from Java side. */
    // These are already implemented in base class by unity
    // public override string toString() => ToString();
    // public override int hashCode() => GetHashCode();
    // public bool equals(object o) => this == o;
  }

  public class JavaListenerProxy : JavaProxy {
    protected JavaListenerProxy(string javaInterface) : base(javaInterface) {}

    protected virtual void invokeOnMain(string methodName, object[] args) => base.Invoke(methodName, args);

    public override AndroidJavaObject Invoke(string methodName, object[] args) {
      ASync.OnMainThread(() => invokeOnMain(methodName, args));
      return null;
    }
  }
}
#endif