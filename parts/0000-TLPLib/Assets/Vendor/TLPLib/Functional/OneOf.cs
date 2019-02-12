using System;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
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
    public readonly A __unsafeGetA;
    public readonly B __unsafeGetB;
    public readonly C __unsafeGetC;
    public readonly OneOf.Choice whichOne;

    public OneOf(A a) {
      __unsafeGetA = a;
      __unsafeGetB = default(B);
      __unsafeGetC = default(C);
      whichOne = OneOf.Choice.A;
    }

    public OneOf(B b) {
      __unsafeGetA = default(A);
      __unsafeGetB = b;
      __unsafeGetC = default(C);
      whichOne = OneOf.Choice.B;
    }

    public OneOf(C c) {
      __unsafeGetA = default(A);
      __unsafeGetB = default(B);
      __unsafeGetC = c;
      whichOne = OneOf.Choice.C;
    }

#region Equality

    public bool Equals(OneOf<A, B, C> other) {
      if (whichOne != other.whichOne) return false;
      switch (whichOne) {
        case OneOf.Choice.A: return EqComparer<A>.Default.Equals(__unsafeGetA, other.__unsafeGetA);
        case OneOf.Choice.B: return EqComparer<B>.Default.Equals(__unsafeGetB, other.__unsafeGetB);
        case OneOf.Choice.C: return EqComparer<C>.Default.Equals(__unsafeGetC, other.__unsafeGetC);
        default: throw new IllegalStateException("Unreachable code");
      }
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is OneOf<A, B, C> oneof && Equals(oneof);
    }

    public override int GetHashCode() {
      switch (whichOne) {
        case OneOf.Choice.A: return EqComparer<A>.Default.GetHashCode(__unsafeGetA);
        case OneOf.Choice.B: return EqComparer<B>.Default.GetHashCode(__unsafeGetB);
        case OneOf.Choice.C: return EqComparer<C>.Default.GetHashCode(__unsafeGetC);
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

    [PublicAPI] public bool isA => whichOne == OneOf.Choice.A;
    [PublicAPI] public Option<A> aValue => isA.opt(__unsafeGetA);
    [PublicAPI] public bool aValueOut(out A value) {
      value = __unsafeGetA;
      return isA;
    }

    [PublicAPI] public bool isB => whichOne == OneOf.Choice.B;
    [PublicAPI] public Option<B> bValue => isB.opt(__unsafeGetB);
    [PublicAPI] public bool bValueOut(out B value) {
      value = __unsafeGetB;
      return isB;
    }

    [PublicAPI] public bool isC => whichOne == OneOf.Choice.C;
    [PublicAPI] public Option<C> cValue => isC.opt(__unsafeGetC);
    [PublicAPI] public bool cValueOut(out C value) {
      value = __unsafeGetC;
      return isC;
    }

    public override string ToString() =>
        isA ? $"OneOf[{typeof(A)}]({__unsafeGetA})"
      : isB ? $"OneOf[{typeof(B)}]({__unsafeGetB})"
            : $"OneOf[{typeof(C)}]({__unsafeGetC})";

    [PublicAPI] public void voidFold(Act<A> onA, Act<B> onB, Act<C> onC) {
      switch (whichOne) {
        case OneOf.Choice.A:
          onA(__unsafeGetA);
          return;
        case OneOf.Choice.B:
          onB(__unsafeGetB);
          return;
        case OneOf.Choice.C:
          onC(__unsafeGetC);
          return;
        default:
          throw new IllegalStateException("Unreachable code");
      }
    }

    [PublicAPI] public R fold<R>(Fn<A, R> onA, Fn<B, R> onB, Fn<C, R> onC) {
      switch (whichOne) {
        case OneOf.Choice.A: return onA(__unsafeGetA);
        case OneOf.Choice.B: return onB(__unsafeGetB);
        case OneOf.Choice.C: return onC(__unsafeGetC);
        default: throw new IllegalStateException("Unreachable code"); 
      }
    }

    [PublicAPI] public OneOf<A, B, C1> mapC<C1>(Func<C, C1> mapper) {
      switch (whichOne) {
        case OneOf.Choice.A: return __unsafeGetA;
        case OneOf.Choice.B: return __unsafeGetB;
        case OneOf.Choice.C: return mapper(__unsafeGetC);
        default: throw new IllegalStateException("Unreachable code"); 
      }
    }

    public static implicit operator OneOf<A, B, C>(A a) => new OneOf<A, B, C>(a);
    public static implicit operator OneOf<A, B, C>(B b) => new OneOf<A, B, C>(b);
    public static implicit operator OneOf<A, B, C>(C c) => new OneOf<A, B, C>(c);
  }
}
