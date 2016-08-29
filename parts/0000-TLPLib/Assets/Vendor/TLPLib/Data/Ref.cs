using System;

namespace com.tinylabproductions.TLPLib.Data {
  public interface Ref<A> {
    A value { get; set; }
  }

  /* Simple heap-allocated reference. */
  public class SimpleRef<A> : Ref<A> {
    public A value { get; set; }

    public SimpleRef(A value) {
      this.value = value;
    }
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
