using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
    /// <summary>Already unsubscribed subscription.</summary>
    public static readonly ISubscription empty = new Subscription(null);

    [CanBeNull] Action onUnsubscribe;

    public bool isSubscribed => onUnsubscribe != null;

    public Subscription(Action onUnsubscribe) {
      this.onUnsubscribe = onUnsubscribe;
    }

    public bool unsubscribe() {
      if (onUnsubscribe != null) {
        onUnsubscribe();
        // Nulling this allows garbage collector to collect whatever is referenced
        // by given action even if someone still has a reference to the subscription
        // itself.
        onUnsubscribe = null;
        return true;
      }
      return false;
    }

    public void Dispose() => unsubscribe();
  }
}
