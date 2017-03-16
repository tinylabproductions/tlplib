using System;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface IObserver {
    void finish();
  }

  public interface IObserver<in A> : IObserver {
    void push(A value);
  }

  public class Observer<A> : IObserver<A> {
    readonly Act<A> onValuePush;
    readonly Action onFinish;

    public Observer(Act<A> onValuePush, Action onFinish) {
      this.onValuePush = onValuePush;
      this.onFinish = onFinish;
    }

    public Observer(Act<A> onValuePush) : this(onValuePush, () => {}) {}

    public void push(A value) => onValuePush(value);
    public void finish() => onFinish();
  }

  public static class IObserverExts {
    public static void pushMany<A>(this IObserver<A> obs, params A[] items) {
      foreach (var a in items) obs.push(a);
    }
  }
}