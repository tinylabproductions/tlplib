#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class System : IDisposable {
    public static A with<A>(Func<System, A> f) {
      using (var sys = new System(new AndroidJavaClass("java.lang.System"))) {
        return f(sys);
      }
    }

    readonly AndroidJavaClass klass;

    System(AndroidJavaClass klass) { this.klass = klass; }

    public void Dispose() => klass.Dispose();

    public Option<string> getProperty(string key) =>
      F.opt(klass.CallStatic<string>("getProperty", key));
  }
}
#endif