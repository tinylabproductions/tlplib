#if UNITY_ANDROID
using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.fns {
  public class Fn1<A> : JavaProxy {
    readonly Func<A> f;

    public Fn1(Func<A> f) : base("com.tinylabproductions.tlplib.fns.Fn1") { this.f = f; }

    [UsedImplicitly]
    A run() => f();
  }
}
#endif