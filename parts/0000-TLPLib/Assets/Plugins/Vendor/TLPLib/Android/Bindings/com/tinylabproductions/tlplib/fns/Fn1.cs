#if UNITY_ANDROID
using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.fns {
  public class Fn1<A> : JavaProxy {
    readonly Fn<A> f;

    public Fn1(Fn<A> f) : base("com.tinylabproductions.tlplib.fns.Fn1") { this.f = f; }

    [UsedImplicitly]
    A run() => f();
  }
}
#endif