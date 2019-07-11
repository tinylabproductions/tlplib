using System;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class OneOf5 {
    public enum Choice : byte { A, B, C, D, E }
  }

  [PublicAPI] 
  public
#if ENABLE_IL2CPP
    class
#else
    struct
#endif
    OneOf<A, B, C, D, E> : IEquatable<OneOf<A, B, C, D, E>>
  {
    readonly A _aValue;
    readonly B _bValue;
    readonly C _cValue;
    readonly D _dValue;
    readonly E _eValue;
    public readonly OneOf5.Choice whichOne;
    
#if ENABLE_IL2CPP
    OneOf() {}
#endif

    public OneOf(A a) : this() {
      _aValue = a;
      whichOne = OneOf5.Choice.A;
    }

    public OneOf(B b) : this() {
      _bValue = b;
      whichOne = OneOf5.Choice.B;
    }

    public OneOf(C c) : this()  {
      _cValue = c;
      whichOne = OneOf5.Choice.C;
    }

    public OneOf(D d) : this() {
      _dValue = d;
      whichOne = OneOf5.Choice.D;
    }

    public OneOf(E e) : this() {
      _eValue = e;
      whichOne = OneOf5.Choice.E;
    }

#region Equality

    public bool Equals(OneOf<A, B, C, D, E> other) {
#if ENABLE_IL2CPP
      if (other == null) return false;
#endif
      if (whichOne != other.whichOne) return false;
      switch (whichOne) {
        case OneOf5.Choice.A: return EqComparer<A>.Default.Equals(_aValue, other._aValue);
        case OneOf5.Choice.B: return EqComparer<B>.Default.Equals(_bValue, other._bValue);
        case OneOf5.Choice.C: return EqComparer<C>.Default.Equals(_cValue, other._cValue);
        case OneOf5.Choice.D: return EqComparer<D>.Default.Equals(_dValue, other._dValue);
        case OneOf5.Choice.E: return EqComparer<E>.Default.Equals(_eValue, other._eValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is OneOf<A, B, C, D> oneOf && Equals(oneOf);
    }

    public override int GetHashCode() {
      switch (whichOne) {
        case OneOf5.Choice.A: return EqComparer<A>.Default.GetHashCode(_aValue);
        case OneOf5.Choice.B: return EqComparer<B>.Default.GetHashCode(_bValue);
        case OneOf5.Choice.C: return EqComparer<C>.Default.GetHashCode(_cValue);
        case OneOf5.Choice.D: return EqComparer<D>.Default.GetHashCode(_dValue);
        case OneOf5.Choice.E: return EqComparer<E>.Default.GetHashCode(_eValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public static bool operator ==(OneOf<A, B, C, D, E> lhs, OneOf<A, B, C, D, E> rhs) {
#if ENABLE_IL2CPP
      var leftNull = ReferenceEquals(lhs, null);
      var rightNull = ReferenceEquals(rhs, null);
      if (leftNull && rightNull) return true;
      if (leftNull || rightNull) return false;
#endif
      return lhs.Equals(rhs);
    }

    public static bool operator !=(OneOf<A, B, C, D, E> lhs, OneOf<A, B, C, D, E> rhs) => !(lhs == rhs);

#endregion

    public bool isA => whichOne == OneOf5.Choice.A;
    public Option<A> aValue => isA.opt(_aValue);
    internal A __unsafeGetA => _aValue;

    public bool isB => whichOne == OneOf5.Choice.B;
    public Option<B> bValue => isB.opt(_bValue);
    internal B __unsafeGetB => _bValue;

    public bool isC => whichOne == OneOf5.Choice.C;
    public Option<C> cValue => isC.opt(_cValue);
    internal C __unsafeGetC => _cValue;

    public bool isD => whichOne == OneOf5.Choice.D;
    public Option<D> dValue => isD.opt(_dValue);
    internal D __unsafeGetD => _dValue;

    public bool isE => whichOne == OneOf5.Choice.E;
    public Option<E> eValue => isE.opt(_eValue);
    internal E __unsafeGetE => _eValue;

    public override string ToString() {
      switch (whichOne) {
        case OneOf5.Choice.A: return $"OneOf[{typeof(A)}]({_aValue})";
        case OneOf5.Choice.B: return $"OneOf[{typeof(B)}]({_bValue})";
        case OneOf5.Choice.C: return $"OneOf[{typeof(C)}]({_cValue})";
        case OneOf5.Choice.D: return $"OneOf[{typeof(D)}]({_dValue})";
        case OneOf5.Choice.E: return $"OneOf[{typeof(E)}]({_eValue})";
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public void voidFold(Action<A> onA, Action<B> onB, Action<C> onC, Action<D> onD, Action<E> onE) {
      switch (whichOne) {
        case OneOf5.Choice.A:
          onA(_aValue);
          return;
        case OneOf5.Choice.B:
          onB(_bValue);
          return;
        case OneOf5.Choice.C:
          onC(_cValue);
          return;
        case OneOf5.Choice.D:
          onD(_dValue);
          return;
        case OneOf5.Choice.E:
          onE(_eValue);
          return;
        default:
          throw new IllegalStateException("Unreachable code");
      }
    }

    public R fold<R>(Func<A, R> onA, Func<B, R> onB, Func<C, R> onC, Func<D, R> onD, Func<E, R> onE) {
      switch (whichOne) {
        case OneOf5.Choice.A: return onA(_aValue);
        case OneOf5.Choice.B: return onB(_bValue);
        case OneOf5.Choice.C: return onC(_cValue);
        case OneOf5.Choice.D: return onD(_dValue);
        case OneOf5.Choice.E: return onE(_eValue);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public static implicit operator OneOf<A, B, C, D, E>(A a) => new OneOf<A, B, C, D, E>(a);
    public static implicit operator OneOf<A, B, C, D, E>(B b) => new OneOf<A, B, C, D, E>(b);
    public static implicit operator OneOf<A, B, C, D, E>(C c) => new OneOf<A, B, C, D, E>(c);
    public static implicit operator OneOf<A, B, C, D, E>(D d) => new OneOf<A, B, C, D, E>(d);
    public static implicit operator OneOf<A, B, C, D, E>(E e) => new OneOf<A, B, C, D, E>(e);
  }
}
