using System;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class LazyValExts {
    public static LazyVal<B> map<A, B>(this LazyVal<A> lazy, Fn<A, B> mapper) => 
      F.lazy(() => mapper(lazy.get));
  }

  // Not `Lazy<A>` because of `System.Lazy<A>`.
  public interface LazyVal<out A> {
    bool initialized { get; }
    A get { get; }
    // For those cases where we want it happen as a side effect.
    A getM();
  }

  public class NotReallyLazyVal<A> : LazyVal<A> {
    public bool initialized { get; } = true;
    public A get { get; }
    public A getM() => get;

    public NotReallyLazyVal(A get) { this.get = get; }
  }

  public class LazyValImpl<A> : LazyVal<A> {
    A obj;
    public bool initialized { get; private set; }
    readonly Fn<A> initializer;

    public LazyValImpl(Fn<A> initializer) {
      this.initializer = initializer;
    }

    public A get { get {
      if (! initialized) {
        obj = initializer();
        initialized = true;
      }
      return obj;
    } }

    public A getM() { return get; }
  }
}
