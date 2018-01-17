using System;
using com.tinylabproductions.TLPLib.dispose;
using WeakReference = com.tinylabproductions.TLPLib.system.WeakReference;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   * 
   * Because it is immutable, the only way for it to change is if its source changes.
   **/
  public interface IRxVal<out A> : IObservable<A> {
    A value { get; }
    ISubscription subscribeWithoutEmit(IDisposableTracker tracker, Act<A> onEvent);
  }
  
  /// <summary>
  /// Reference that has a current value and is based on another <see cref="IObservable{A}"/>.
  /// 
  /// DO NOT initiate this yourself!
  /// 
  /// Use the provided operations like <see cref="RxValOps.map{A,B}"/>.
  /// </summary>
  public class RxVal<A> : IRxVal<A> {
    public delegate void SetValue(A a);
    
    public int subscribers => rxRef.subscribers;
    public A value => rxRef.value;
    
    readonly IRxRef<A> rxRef;
    
    // ReSharper disable once NotAccessedField.Local
    // This subscription is kept here to have a hard reference to the source.
    readonly ISubscription baseObservableSubscription;

    public RxVal(A initialValue, Fn<SetValue, ISubscription> subscribeToSource) {
      rxRef = RxRef.a(initialValue);
      var wr = WeakReference.a(this);
      var sub = Subscription.empty;
      sub = subscribeToSource(
        // This callback goes into the source observable callback list, therefore
        // we want to be very careful here to not capture this reference, to avoid
        // establishing a circular strong reference.
        //
        //                       +--------------+                               
        //        +--------------- Subscription <------------------------+      
        //        |              +---^----------+                        |      
        // +------v-----+            |                                   |      
        // | Source     -------------+     +----------+                +-|-----+
        // | observable -------------------> Callback -----------------> RxVal |
        // +------------+                  +----------+  weak          +-------+
        //                                               reference
        //
        // All the hard references should point backwards.
        a => {
          // Make sure to not capture `this`!
          var thizOpt = wr.Target;
          if (thizOpt.isSome) thizOpt.__unsafeGetValue.rxRef.value = a;
          else sub.unsubscribe();
        }
      );
      baseObservableSubscription = sub;
    }

    public ISubscription subscribe(IDisposableTracker tracker, Act<A> onEvent) =>
      rxRef.subscribe(tracker, onEvent);

    public ISubscription subscribeWithoutEmit(IDisposableTracker tracker, Act<A> onEvent) =>
      rxRef.subscribeWithoutEmit(tracker, onEvent);
  }

  public static class RxVal {
    #region Constructors

    /* Never changing RxVal. Useful for lifting values into reactive values. */
    public static IRxVal<A> a<A>(A value) => RxValStatic.a(value);
    public static IRxVal<A> cached<A>(A value) => RxValCache<A>.get(value);

    #endregion
  }
}