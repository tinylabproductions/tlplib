using System;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   * 
   * Because it is immutable, the only way for it to change is if its source changes.
   **/
  public interface IRxVal<out A> : Val<A>, IObservable<A> {
    uint valueVersion { get; }
    ISubscription subscribe(IObserver<A> observer, bool submitCurrentValue);
  }
  
  public class RxVal<A> : RxBase<A>, IRxVal<A> {
    public struct SourceProperties {
      public readonly Fn<bool> needsUpdate;
      public readonly Fn<A> getCurrentValue;

      public SourceProperties(Fn<bool> needsUpdate, Fn<A> getCurrentValue) {
        this.needsUpdate = needsUpdate;
        this.getCurrentValue = getCurrentValue;
      }
    }

    readonly SourceProperties sourceProperties;

    public RxVal(
      SourceProperties sourceProperties, SubscribeToSource<A> subscribeFn
    ) : base(default(A), subscribeFn) {
      this.sourceProperties = sourceProperties;
    }

    protected override A value {
      get {
        if (sourceProperties.needsUpdate())
          base.value = sourceProperties.getCurrentValue();
        return base.value;
      }
    }

    A Val<A>.value => value;

    public override string ToString() => $"{nameof(RxVal)}({value})";
  }

  public static class RxVal {
    /* Never changing RxVal. Useful for lifting values into reactive values. */
    public static IRxVal<A> a<A>(A value) => RxValStatic.a(value);
    public static IRxVal<A> cached<A>(A value) => RxValCache<A>.get(value);

    /* RxVal that gets its value from other reactive source where the value is always available. */
    public static IRxVal<A> a<A>(RxVal<A>.SourceProperties sourceProperties, SubscribeToSource<A> subscribeFn) => 
      new RxVal<A>(sourceProperties, subscribeFn);
  }
}