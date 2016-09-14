using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubscription {
    bool isSubscribed { get; }
    bool unsubscribe();
    ISubscription andThen(Action action);
    ISubscription join(params ISubscription[] other);
    ISubscription joinEnum(IEnumerable<ISubscription> others);
  }

  public class Subscription : ISubscription {
    readonly Action onUnsubscribe;

    public bool isSubscribed { get; private set; } = true;

    public static ISubscription a(Action onUnsubscribe) {
      return new Subscription(onUnsubscribe);
    }

    public Subscription(Action onUnsubscribe) {
      this.onUnsubscribe = onUnsubscribe;
    }

    public bool unsubscribe() {
      if (!isSubscribed) return false;
      isSubscribed = false;
      onUnsubscribe();
      return true;
    }

    public ISubscription andThen(Action action) {
      return new Subscription(() => {
        unsubscribe();
        action();
      });
    }

    public ISubscription join(params ISubscription[] other) => joinEnum(other);

    public ISubscription joinEnum(IEnumerable<ISubscription> others) {
      return new Subscription(() => {
        unsubscribe();
        foreach (var other in others) other.unsubscribe();
      });
    }
  }

  public static class ISubscriptionExts {
    public static ISubscription joinSubscriptions(
      this IEnumerable<ISubscription> subscriptions
    ) => new Subscription(() => {
      foreach (var sub in subscriptions) sub.unsubscribe();
    });
  }

  public class SubscriptionTracker : IDisposable {
    readonly List<ISubscription> subscriptions = new List<ISubscription>();

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
