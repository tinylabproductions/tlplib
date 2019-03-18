using System;
using com.tinylabproductions.TLPLib.Extensions;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class OneOf4 {
    public enum Choice : byte { A, B, C, D }
  }

  public
#if ENABLE_IL2CPP
    class
#else
    struct
#endif
    OneOf<A, B, C, D> : IEquatable<OneOf<A, B, C, D>>
  {
    readonly A _aValue;
    readonly B _bValue;
    readonly C _cValue;
    readonly D _dValue;
    public readonly OneOf4.Choice whichOne;

    public OneOf(A a) {
      _aValue = a;
      _bValue = default(B);
      _cValue = default(C);
      _dValue = default(D);
      whichOne = OneOf4.Choice.A;
    }

    public OneOf(B b) {
      _aValue = default(A);
      _bValue = b;
      _cValue = default(C);
      _dValue = default(D);
      whichOne = OneOf4.Choice.B;
    }

    public OneOf(C c) {
      _aValue = default(A);
      _bValue = default(B);
      _cValue = c;
      _dValue = default(D);
      whichOne = OneOf4.Choice.C;
    }

    public OneOf(D d) {
      _aValue = default(A);
      _bValue = default(B);
      _cValue = default(C);
      _dValue = d;
      whichOne = OneOf4.Choice.D;
    }

#region Equality

    public bool Equals(OneOf<A, B, C, D> other) {
      if (whichOne != other.whichOne) return false;
      switch (whichOne) {
        case OneOf4.Choice.A: return EqComparer<A>.Default.Equals(_aValue, other._aValue);
        case OneOf4.Choice.B: return EqComparer<B>.Default.Equals(_bValue, other._bValue);
        case OneOf4.Choice.C: return EqComparer<C>.Default.Equals(_cValue, other._cValue);
        case OneOf4.Choice.D: return EqComparer<D>.Default.Equals(_dValue, other._dValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is OneOf<A, B, C, D> oneof && Equals(oneof);
    }

    public override int GetHashCode() {
      switch (whichOne) {
        case OneOf4.Choice.A: return EqComparer<A>.Default.GetHashCode(_aValue);
        case OneOf4.Choice.B: return EqComparer<B>.Default.GetHashCode(_bValue);
        case OneOf4.Choice.C: return EqComparer<C>.Default.GetHashCode(_cValue);
        case OneOf4.Choice.D: return EqComparer<D>.Default.GetHashCode(_dValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public static bool operator ==(OneOf<A, B, C, D> lhs, OneOf<A, B, C, D> rhs) {
#if ENABLE_IL2CPP
      var leftNull = ReferenceEquals(lhs, null);
      var rightNull = ReferenceEquals(rhs, null);
      if (leftNull && rightNull) return true;
      if (leftNull || rightNull) return false;
#endif
      return lhs.Equals(rhs);
    }

    public static bool operator !=(OneOf<A, B, C, D> lhs, OneOf<A, B, C, D> rhs) => !(lhs == rhs);

#endregion

    public bool isA => whichOne == OneOf4.Choice.A;
    public Option<A> aValue => isA.opt(_aValue);
    internal A __unsafeGetA => _aValue;

    public bool isB => whichOne == OneOf4.Choice.B;
    public Option<B> bValue => isB.opt(_bValue);
    internal B __unsafeGetB => _bValue;

    public bool isC => whichOne == OneOf4.Choice.C;
    public Option<C> cValue => isC.opt(_cValue);
    internal C __unsafeGetC => _cValue;

    public bool isD => whichOne == OneOf4.Choice.D;
    public Option<D> dValue => isD.opt(_dValue);
    internal D __unsafeGetD => _dValue;

    public override string ToString() {
      switch (whichOne) {
        case OneOf4.Choice.A: return $"OneOf[{typeof(A)}]({_aValue})";
        case OneOf4.Choice.B: return $"OneOf[{typeof(B)}]({_bValue})";
        case OneOf4.Choice.C: return $"OneOf[{typeof(C)}]({_cValue})";
        case OneOf4.Choice.D: return $"OneOf[{typeof(D)}]({_dValue})";
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public void voidFold(Action<A> onA, Action<B> onB, Action<C> onC, Action<D> onD) {
      switch (whichOne) {
        case OneOf4.Choice.A:
          onA(_aValue);
          return;
        case OneOf4.Choice.B:
          onB(_bValue);
          return;
        case OneOf4.Choice.C:
          onC(_cValue);
          return;
        case OneOf4.Choice.D:
          onD(_dValue);
          return;
        default:
          throw new IllegalStateException("Unreachable code");
      }
    }

    public R fold<R>(Func<A, R> onA, Func<B, R> onB, Func<C, R> onC, Func<D, R> onD) {
      switch (whichOne) {
        case OneOf4.Choice.A: return onA(_aValue);
        case OneOf4.Choice.B: return onB(_bValue);
        case OneOf4.Choice.C: return onC(_cValue);
        case OneOf4.Choice.D: return onD(_dValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public static implicit operator OneOf<A, B, C, D>(A a) => new OneOf<A, B, C, D>(a);
    public static implicit operator OneOf<A, B, C, D>(B b) => new OneOf<A, B, C, D>(b);
    public static implicit operator OneOf<A, B, C, D>(C c) => new OneOf<A, B, C, D>(c);
    public static implicit operator OneOf<A, B, C, D>(D d) => new OneOf<A, B, C, D>(d);
  }
}
