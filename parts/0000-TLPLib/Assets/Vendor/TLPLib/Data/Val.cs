using System;

namespace com.tinylabproductions.TLPLib.Data {
  public interface Val<out A> {
    A value { get; }
  }

  public static class Val {
    public static Val<A> a<A>(Func<A> get) => new LambdaVal<A>(get);
  }

  public static class ValExts {
    public static Val<B> mapVal<A, B>(this Val<A> v, Func<A, B> mapper) =>
      new LambdaVal<B>(() => mapper(v.value));
  }

  public class LambdaVal<A> : Val<A> {
    readonly Func<A> get;

    public LambdaVal(Func<A> get) {
      this.get = get;
    }

    public A value => get();
  }
}