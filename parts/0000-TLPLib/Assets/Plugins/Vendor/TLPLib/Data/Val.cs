using System;

namespace com.tinylabproductions.TLPLib.Data {
  public interface Val<out A> {
    A value { get; }
  }

  public static class Val {
    public static Val<A> a<A>(Fn<A> get) => new LambdaVal<A>(get);
  }

  public static class ValExts {
    public static Val<B> mapVal<A, B>(this Val<A> v, Fn<A, B> mapper) =>
      new LambdaVal<B>(() => mapper(v.value));
  }

  public class LambdaVal<A> : Val<A> {
    readonly Fn<A> get;

    public LambdaVal(Fn<A> get) {
      this.get = get;
    }

    public A value => get();
  }
}