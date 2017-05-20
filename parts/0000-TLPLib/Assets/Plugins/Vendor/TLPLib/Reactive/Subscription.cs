using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubscription : IDisposable {
    bool isSubscribed { get; }
    bool unsubscribe();
  }

  public static class ISubscriptionExts {
    public static ISubscription andThen(this ISubscription sub, Action action) => 
      new Subscription(() => {
        sub.unsubscribe();
        action();
      });

    public static ISubscription join(this ISubscription sub1, ISubscription sub2) =>
      sub1.andThen(() => sub2.unsubscribe());

    public static ISubscription join(this ISubscription sub, params ISubscription[] other) =>
      sub.joinEnum(other);

    public static ISubscription joinEnum(this ISubscription sub, IEnumerable<ISubscription> others) =>
      new Subscription(() => {
        sub.unsubscribe();
        foreach (var other in others) other.unsubscribe();
      });

    public static ISubscription joinSubscriptions(
      this IEnumerable<ISubscription> subscriptions
    ) => new Subscription(() => {
      foreach (var sub in subscriptions) sub.unsubscribe();
    });
  }


  public class Subscription : ISubscription {
    /** Already unsubscribed subscription. */
    public static readonly ISubscription empty;

    static Subscription() {
      empty = new Subscription(() => {});
      empty.unsubscribe();
    }

    public static ISubscription a(Action onUnsubscribe) => new Subscription(onUnsubscribe);

    readonly Action onUnsubscribe;

    public bool isSubscribed { get; private set; } = true;

    public Subscription(Action onUnsubscribe) {
      this.onUnsubscribe = onUnsubscribe;
    }

    public bool unsubscribe() {
      if (!isSubscribed) return false;
      isSubscribed = false;
      onUnsubscribe();
      return true;
    }

    public void Dispose() => unsubscribe();
  }
}
