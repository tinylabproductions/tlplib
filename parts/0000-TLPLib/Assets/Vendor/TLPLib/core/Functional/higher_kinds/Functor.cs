using System;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;
using pzd.lib.functional.higher_kinds;

namespace com.tinylabproductions.TLPLib.Functional.higher_kinds {
  [PublicAPI] public class FunctorsU : Functor<Future.W> {
    public static readonly FunctorsU i = new FunctorsU();
    FunctorsU() {}
    
    public HigherKind<Future.W, B> map<A, B>(HigherKind<Future.W, A> data, Func<A, B> mapper) =>
      data.narrowK().map(mapper);
  }
}