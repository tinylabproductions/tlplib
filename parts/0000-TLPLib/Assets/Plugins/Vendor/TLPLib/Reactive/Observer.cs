using System;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface IObserver<in A> {
    void push(A value);
  }

  public class Observer<A> : IObserver<A> {
    readonly Act<A> onValuePush;

    public Observer(Act<A> onValuePush) {
      this.onValuePush = onValuePush;
    }

    public void push(A value) => onValuePush(value);
  }

  public static class IObserverExts {
    public static void pushMany<A>(this IObserver<A> obs, params A[] items) {
      foreach (var a in items) obs.push(a);
    }
  }
}