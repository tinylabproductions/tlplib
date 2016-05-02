using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubscription {
    bool isSubscribed { get; }
    bool unsubscribe();
    ISubscription andThen(Act action);
    ISubscription join(params ISubscription[] other);
  }

  public class Subscription : ISubscription {
    private readonly Act onUnsubscribe;
    private bool _isSubscribed = true;

    public static ISubscription a(Act onUnsubscribe) {
      return new Subscription(onUnsubscribe);
    }

    public Subscription(Act onUnsubscribe) {
      this.onUnsubscribe = onUnsubscribe;
    }

    public bool isSubscribed { get { return _isSubscribed; } }

    public bool unsubscribe() {
      if (!isSubscribed) return false;
      _isSubscribed = false;
      onUnsubscribe();
      return true;
    }

    public ISubscription andThen(Act action) {
      return new Subscription(() => {
        unsubscribe();
        action();
      });
    }

    public ISubscription join(params ISubscription[] other) {
      return new Subscription(() => {
        unsubscribe();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var idx = 0; idx < other.Length; idx++)
          other[idx].unsubscribe();
      });
    }
  }

  public class SubscriptionTracker : IDisposable {
    private readonly List<ISubscription> subscriptions =
      new List<ISubscription>();

    public ISubscription track(ISubscription subscription) {
      subscriptions.Add(subscription);
      return subscription;
    }

    public void Dispose() {
      foreach (var subscription in subscriptions)
        subscription.unsubscribe();
      subscriptions.Clear();
    }
  }
}
