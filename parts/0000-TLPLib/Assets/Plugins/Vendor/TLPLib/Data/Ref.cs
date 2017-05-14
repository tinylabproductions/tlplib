using System;

namespace com.tinylabproductions.TLPLib.Data {
  public interface Ref<A> : Val<A> {
    new A value { get; set; }
  }

  /* Simple heap-allocated reference. */
  public sealed class SimpleRef<A> : Ref<A> {
    // For access using ref keyword.
    public A value;

    A Val<A>.value => value;

    A Ref<A>.value {
      get { return value; }
      set { this.value = value; }
    }

    public SimpleRef(A value) { this.value = value; }

    public override string ToString() => $"{nameof(SimpleRef<A>)}({value})";
  }

  public class LambdaRef<A> : Ref<A> {
    readonly Fn<A> get;
    readonly Act<A> set;

    public LambdaRef(Fn<A> get, Act<A> set) {
      this.get = get;
      this.set = set;
    }

    public A value {
      get { return get(); }
      set { set(value); }
    }
  }

  public static class Ref {
    public static Ref<A> a<A>(A value) => new SimpleRef<A>(value);
    public static Ref<A> a<A>(Fn<A> get, Act<A> set) => new LambdaRef<A>(get, set);
  }
}
