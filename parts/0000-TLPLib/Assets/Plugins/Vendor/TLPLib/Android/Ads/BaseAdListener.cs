using System;
using com.tinylabproductions.TLPLib.Concurrent;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
  /**
   * Callbacks are usually not fired on a main thread, thus we need to reschedule them.
   *
   * Sidenote:
   *
   * We can't do: 
   * <code>
   *   public static void invoke(Action a) { if (a != null) ASync.OnMainThread(a); }
   * </code>
   *
   * Because, given following situation:
   *
   * Listener code:
   * <code>
   *   void onJavaEvt1() => invoke(evt1);
   *   void onJavaEvt2() => invoke(evt2);
   * </code>
   *
   * <code>
   *   Future<bool> f;
   *   Promise<bool> p;
   *   Action onEvt1 = () => p.complete(true);
   *   Action onEvt2 = () => p.complete(false);
   *   listener.evt1 += onEvt1;
   *   listener.evt2 += onEvt2;
   *   f.onComplete(_ => {
   *     listener.evt1 -= onEvt1;
   *     listener.evt2 -= onEvt2;
   *   });
   *   runCode();
   * </code>
   *
   * If `evt1` and `evt2` fires in succession, second event would fail with future already completed.
   * 
   * This happens because at the time of calling `invoke` we take the Action assigned  to the listener and
   * store it in a closure, thus the `-=` does not take effect. We have to delay getting the action to run
   * until the last possible time, thus the mandatory `OnMainThread`.
   **/
  public static class BaseAdListenerOps {
    public static void invoke(Fn<Action> aFn) => 
      ASync.OnMainThread(() => aFn()?.Invoke());

    public static void invoke<A>(Fn<Act<A>> act, A a) => 
      ASync.OnMainThread(() => act()?.Invoke(a));

    public static void invoke<A, B>(Fn<Act<A, B>> act, A a, B b) => 
      ASync.OnMainThread(() => act()?.Invoke(a, b));
  }

#if UNITY_ANDROID
  public abstract class BaseAdListener : JavaProxy {
    protected BaseAdListener(string javaInterface) : base(javaInterface) {}
    protected BaseAdListener(AndroidJavaClass javaInterface) : base(javaInterface) {}

    public static void invoke(Fn<Action> aFn) => BaseAdListenerOps.invoke(aFn);
    public static void invoke<A>(Fn<Act<A>> act, A a) => BaseAdListenerOps.invoke(act, a);
    public static void invoke<A, B>(Fn<Act<A, B>> act, A a, B b) => BaseAdListenerOps.invoke(act, a, b);
  }
#endif
}