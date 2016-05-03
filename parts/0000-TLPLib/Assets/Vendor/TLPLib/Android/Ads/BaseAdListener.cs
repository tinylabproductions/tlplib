using System;
using com.tinylabproductions.TLPLib.Concurrent;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
#if UNITY_ANDROID
  public abstract class BaseAdListener : AndroidJavaProxy {
    protected BaseAdListener(string javaInterface) : base(javaInterface) {}
    protected BaseAdListener(AndroidJavaClass javaInterface) : base(javaInterface) {}

    /* Callbacks are usually not fired from main Unity thread. */
    public static void invoke(Action a) { if (a != null) ASync.OnMainThread(() => a()); }

    public static void invoke<A>(Action<A> act, A a)
      { if (act != null) ASync.OnMainThread(() => act.Invoke(a)); }

    public static void invoke<A, B>(Action<A, B> act, A a, B b)
      { if (act != null) ASync.OnMainThread(() => act.Invoke(a, b)); }
  }
#endif
}