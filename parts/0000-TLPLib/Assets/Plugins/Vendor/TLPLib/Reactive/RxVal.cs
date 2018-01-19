using System;
using com.tinylabproductions.TLPLib.dispose;
using WeakReference = com.tinylabproductions.TLPLib.system.WeakReference;

namespace com.tinylabproductions.TLPLib.Reactive {
  /// <summary>
  /// RxVal is an observable which has a current value.
  /// 
  /// Because it is immutable, the only way for it to change is if its source changes.
  /// </summary>
  /// <typeparam name="A"></typeparam>
  public interface IRxVal<out A> : IObservable<A> {
    A value { get; }
    ISubscription subscribeWithoutEmit(IDisposableTracker tracker, Act<A> onEvent);
  }
  
  /// <summary>
  /// <para>
  /// Reference that has a current value and is based on another <see cref="IObservable{A}"/>.
  /// </para>
  /// 
  /// <para>
  /// Usually you would not create it directly, but use operations like
  /// <see cref="RxValOps.map{A,B}"/> or <see cref="RxValOps.flatMap{A,B}"/>.
  /// </para>
  /// 
  /// <para>
  /// You should note that the transformation functions should be side effect free.
  /// </para>
  /// 
  /// <para>
  /// If you want to do side effects in your transformation functions, you should understand
  /// that it is a bit complicated, as we do not manage the memory ourselves.
  /// </para>
  /// 
  /// <para>
  /// Lets say you have this:
  /// </para>
  /// 
  /// <code><![CDATA[
  ///   IRxVal<Level> transform(IRxVal<int> levelIdRx) =>
  ///     // We should destroy old level when src changes, but it is ommited for brevity. 
  ///     levelIdRx.map(f: levelId => instantiateLevel(levelId)); 
  /// ]]></code>
  /// 
  /// <para>
  /// Because level <see cref="RxVal{A}"/> needs a value immediately (because anyone can ask it
  /// for one at any time), it runs the mapper upon creation, doing the side effect (instantiating
  /// the level). This might or might not be intended.
  /// </para>
  /// 
  /// <para>
  /// The mapper will be once ran for every levelIdRx change as well.
  /// </para>
  /// 
  /// <para>
  /// Now if you lose the reference to old level rx value and decide to run transform again,
  /// memory will look like this in a bit:
  /// </para>
  /// 
  /// <code><![CDATA[
  /// 
  ///  +--------------+                                             
  ///  | Subscription |---------------------------------+           
  ///  +-------^------+                                 |           
  ///          |                                        |           
  ///          |                                        |           
  /// +--------v--+   +----+   +----------+ weakref +---|----------+
  /// | levelIdRx |---+ f  ----> setValue - - - - - > RxVal<Level> |
  /// +--------^--|   +----+   +----------+         +--------------+
  ///          |  |                                                 
  ///          |  |   +----+   +----------+ weakref +--------------+
  ///          |  +---> f  ----+ setValue - - - - - > RxVal<Level> |<---- you hold a reference to this
  ///          |      +----+   +----------+         +---|----------+
  ///          |                                        | 
  ///  +-------v------+                                 |           
  ///  | Subscription <---------------------------------+           
  ///  +--------------+
  /// ]]></code>
  /// 
  /// <para>
  /// The garbage collector will collect the loose RxVal eventually, but because it is non-deterministic
  /// we do not know when exactly will that happen.
  /// </para>
  /// 
  /// <para>
  /// The consequence is that if we would update the levelIdRx before the garbage
  /// collection happens, our mapper function would be executed twice. And because it
  /// does side effects, those side effects would happen twice as well, leading to two
  /// instances of the Level.
  /// </para>
  /// 
  /// <para>
  /// We recommend that instead of doing side effects in transformation functions, you would do
  /// them only in subscription functions, where the subscription lifetimes are explicitly handled
  /// by you.
  /// </para>
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
        // | observable -------------------> Callback - - - - - - - - -> RxVal |
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

    /// <summary>
    /// Never changing RxVal. Useful for lifting values into reactive values.
    /// </summary>
    public static IRxVal<A> a<A>(A value) => RxValStatic.a(value);
    public static IRxVal<A> cached<A>(A value) => RxValCache<A>.get(value);

    #endregion
  }
}