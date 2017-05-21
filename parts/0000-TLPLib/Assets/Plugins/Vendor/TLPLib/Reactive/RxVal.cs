using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Logger;
using WeakReference = com.tinylabproductions.TLPLib.system.WeakReference;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   * 
   * Because it is immutable, the only way for it to change is if its source changes.
   **/
  public interface IRxVal<out A> : Val<A>, IObservable<A> {
    ISubscription subscribe(IObserver<A> observer, bool submitCurrentValue);
  }

  public class RxVal<A> : RxBase<A>, IRxVal<A> {
    /***
     * Set the value of this RxVal. bool indicates whether a set was successful.
     * 
     * If a set is not successful, you should unsubscribe from the source.
     * 
     * Set might not be successful if this RxVal did not have subscribers and it
     * got garbage collected.
     */
    public delegate bool SetValue(A a);

    readonly SetValue strongRefSetValue, weakRefSetValue;
    readonly Fn<SetValue, ISubscription> subscribeToSource;
    readonly Guid guid = Guid.NewGuid();
    ISubscription sourceSubscription;

    /* RxVal that gets its value from other reactive source where the value is always available. */
    public RxVal(A value, Fn<SetValue, ISubscription> subscribeToSource) : base(value) {
      // To always have the newest value, RxVal has to always be subscribed to its source.
      // However, this creates a reference from the source to the destination RxVal.
      //
      // For example given:
      //   bRx = aRx.map(_ => _ * 2);
      //   bRx = null;
      // Now we have a reference pointing from aRx callbacks list to bRx, which prevents from
      // bRx ever being garbage collected, even though we have no other references.
      //
      // In such scenario, we want the a -> b reference to be weak, not preventing GC.
      var wr = WeakReference.a(this);
      weakRefSetValue = a => {
        foreach (var self in wr.Target) {
          self.value = a;
          return true;
        }
        return false;
      };
      // However when we have subscribers, we want to make sure side effects happen in such 
      // scenario:
      //   aRx.map(_ => _ * 2).subscribe(a => obj.text = a.ToString());
      // Even though we do not have an explicit reference stored, as long as the source emits
      // events, we want the side effects to happen. Thus we need to exchange the weak reference
      // with the strong one.
      strongRefSetValue = a => {
        value = a;
        return true;
      };

      this.subscribeToSource = subscribeToSource;
      sourceSubscription = subscribeToSource(weakRefSetValue);

      if (Log.isVerbose) Log.verbose($"Created {this} - {guid}");
    }

    ~RxVal() {
      if (Log.isVerbose) Log.verbose($"Finalizing {this} - {guid}");
    }

    A Val<A>.value => value;
    public override string ToString() => $"{nameof(RxVal)}({value})";

    public override ISubscription subscribe(IObserver<A> observer, bool submitCurrentValue) {
      if (subscribers == 0) {
        sourceSubscription.unsubscribe();
        sourceSubscription = subscribeToSource(strongRefSetValue);
      }

      var sub = base.subscribe(observer, submitCurrentValue);
      return sub.andThen(() => {
        if (subscribers == 0) {
          sourceSubscription.unsubscribe();
          sourceSubscription = subscribeToSource(weakRefSetValue);
        }
      });
    }
  }

  public static class RxVal {
    /* Never changing RxVal. Useful for lifting values into reactive values. */
    public static IRxVal<A> a<A>(A value) => RxValStatic.a(value);
    public static IRxVal<A> cached<A>(A value) => RxValCache<A>.get(value);
  }
}