using System;
using com.tinylabproductions.TLPLib.Extensions;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class OneOf {
    public enum Choice : byte { A, B, C }
  }

  public
#if ENABLE_IL2CPP
    class
#else
    struct
#endif
    OneOf<A, B, C> : IEquatable<OneOf<A, B, C>>
  {
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

#region Equality

    public bool Equals(OneOf<A, B, C> other) {
      if (whichOne != other.whichOne) return false;
      switch (whichOne) {
        case OneOf.Choice.A: return EqComparer<A>.Default.Equals(_aValue, other._aValue);
        case OneOf.Choice.B: return EqComparer<B>.Default.Equals(_bValue, other._bValue);
        case OneOf.Choice.C: return EqComparer<C>.Default.Equals(_cValue, other._cValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is OneOf<A, B, C> oneof && Equals(oneof);
    }

    public override int GetHashCode() {
      switch (whichOne) {
        case OneOf.Choice.A: return EqComparer<A>.Default.GetHashCode(_aValue);
        case OneOf.Choice.B: return EqComparer<B>.Default.GetHashCode(_bValue);
        case OneOf.Choice.C: return EqComparer<C>.Default.GetHashCode(_cValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public static bool operator ==(OneOf<A, B, C> lhs, OneOf<A, B, C> rhs) {
#if ENABLE_IL2CPP
      var leftNull = ReferenceEquals(lhs, null);
      var rightNull = ReferenceEquals(rhs, null);
      if (leftNull && rightNull) return true;
      if (leftNull || rightNull) return false;
#endif
      return lhs.Equals(rhs);
    }

    public static bool operator !=(OneOf<A, B, C> lhs, OneOf<A, B, C> rhs) => !(lhs == rhs);

#endregion

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
        isA ? $"OneOf[{typeof(A)}]({_aValue})"
      : isB ? $"OneOf[{typeof(B)}]({_bValue})"
            : $"OneOf[{typeof(C)}]({_cValue})";

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

    public static implicit operator OneOf<A, B, C>(A a) => new OneOf<A, B, C>(a);
    public static implicit operator OneOf<A, B, C>(B b) => new OneOf<A, B, C>(b);
    public static implicit operator OneOf<A, B, C>(C c) => new OneOf<A, B, C>(c);
  }
}
