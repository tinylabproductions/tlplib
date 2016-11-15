#if UNITY_ANDROID
using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.fns {
  public class Act1<A> : JavaProxy {
    readonly Act<A> act;

    public Act1(Act<A> act) : base("com.tinylabproductions.tlplib.fns.Act1") { this.act = act; }

    [UsedImplicitly]
    void run(A a) => act(a);
  }
}
#endif