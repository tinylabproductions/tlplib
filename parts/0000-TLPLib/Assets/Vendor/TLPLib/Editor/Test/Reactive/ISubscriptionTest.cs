using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
  class ISubscriptionHelpers {
    public static ImmutableList<ISubscription> createSubs() =>
      F.iListFill(10, _ => (ISubscription) new Subscription(() => { }));

    public static void checkSubs(
      ISubscription joined, ICollection<ISubscription> subs
    ) {
      foreach (var sub in subs)
        sub.isSubscribed.shouldBeTrue("Subscriptions should be active initially.");
      joined.unsubscribe();
      foreach (var sub in subs)
        sub.isSubscribed.shouldBeFalse(
          "Subscriptions should be inactive after joined subscription unsubscribes."
        );
    }
  }

  public class ISubscriptionTestJoinSubscriptions {
    [Test]
    public void Test() {
      var subs = ISubscriptionHelpers.createSubs();
      var joined = subs.joinSubscriptions();
      ISubscriptionHelpers.checkSubs(joined, subs);
    }
  }

  public class ISubscriptionTestJoinEnum {
    [Test]
    public void Test() {
      ISubscription sub = new Subscription(() => {});
      var subs = ISubscriptionHelpers.createSubs();
      var joined = sub.joinEnum(subs);
      ISubscriptionHelpers.checkSubs(joined, subs.Add(sub));
    }
  }
}