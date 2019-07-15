using System;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.functional.higher_kinds;

namespace com.tinylabproductions.TLPLib.Functional.higher_kinds {
  public class MonadsU : Monad<Future.W> {
    public static readonly MonadsU i = new MonadsU();
    MonadsU() {}
    
    #region Future

    public HigherKind<Future.W, B> map<A, B>(HigherKind<Future.W, A> data, Func<A, B> mapper) =>
      FunctorsU.i.map(data, mapper);

    HigherKind<Future.W, A> Monad<Future.W>.point<A>(A a) =>
      Future.successful(a);
    
    public HigherKind<Future.W, B> flatMap<A, B>(
      HigherKind<Future.W, A> data, Func<A, HigherKind<Future.W, B>> mapper
    ) => data.narrowK().flatMap(a => mapper(a).narrowK()); 

    #endregion
  }
}