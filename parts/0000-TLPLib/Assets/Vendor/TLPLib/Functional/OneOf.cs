using System;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class OneOf {
    public enum Choice : byte { A, B, C }
  }

  public struct OneOf<A, B, C> {
    readonly A _aValue;
    readonly B _bValue;
    readonly C _cValue;
    public readonly OneOf.Choice whichOne;

    public OneOf(A a) {
      _aValue = a;
      _bValue = default(B);
      _cValue = default(C);
      whichOne = OneOf.Choice.A;
    }

    public OneOf(B b) {
      _aValue = default(A);
      _bValue = b;
      _cValue = default(C);
      whichOne = OneOf.Choice.B;
    }

    public OneOf(C c) {
      _aValue = default(A);
      _bValue = default(B);
      _cValue = c;
      whichOne = OneOf.Choice.C;
    }

    public bool isA => whichOne == OneOf.Choice.A;
    public Option<A> aValue => isA.opt(_aValue);
    internal A __unsafeGetA => _aValue;

    public bool isB => whichOne == OneOf.Choice.B;
    public Option<B> bValue => isB.opt(_bValue);
    internal B __unsafeGetB => _bValue;

    public bool isC => whichOne == OneOf.Choice.C;
    public Option<C> cValue => isC.opt(_cValue);
    internal C __unsafeGetC => _cValue;

    public override string ToString() =>
        isA ? $"OneOf.{typeof(A)}({_aValue})"
      : isB ? $"OneOf.{typeof(B)}({_bValue})"
            : $"OneOf.{typeof(C)}({_cValue})";

    public void voidFold(Act<A> onA, Act<B> onB, Act<C> onC) {
      switch (whichOne) {
        case OneOf.Choice.A:
          onA(_aValue);
          return;
        case OneOf.Choice.B:
          onB(_bValue);
          return;
        case OneOf.Choice.C:
          onC(_cValue);
          return;
      }
      throw new IllegalStateException("Unreachable code");
    }

    public R fold<R>(Fn<A, R> onA, Fn<B, R> onB, Fn<C, R> onC) {
      switch (whichOne) {
        case OneOf.Choice.A: return onA(_aValue);
        case OneOf.Choice.B: return onB(_bValue);
        case OneOf.Choice.C: return onC(_cValue);
      }
      throw new IllegalStateException("Unreachable code");
    }
  }
}
