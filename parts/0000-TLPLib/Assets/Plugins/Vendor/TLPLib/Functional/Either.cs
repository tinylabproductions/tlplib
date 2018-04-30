using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class EitherExts {
    [PublicAPI] public static Either<A, B> flatten<A, B>(this Either<A, Either<A, B>> e) =>
      e.flatMapRight(_ => _);

    static Fn<CollFrom, IEnumerable<ElemTo>> mapC<CollFrom, ElemFrom, ElemTo>(
      Fn<ElemFrom, ElemTo> mapper
    ) where CollFrom : IEnumerable<ElemFrom> =>
      c => c.Select(elem => mapper(elem));

    [PublicAPI] public static Either<IEnumerable<ElemTo>, Right> mapLeftC<CollFrom, Right, ElemFrom, ElemTo>(
      this Either<CollFrom, Right> e,
      Fn<ElemFrom, ElemTo> mapper
    ) where CollFrom : IEnumerable<ElemFrom> =>
      e.mapLeft(mapC<CollFrom, ElemFrom, ElemTo>(mapper));

    [PublicAPI] public static Either<Left, IEnumerable<ElemTo>> mapRightC<CollFrom, Left, ElemFrom, ElemTo>(
      this Either<Left, CollFrom> e,
      Fn<ElemFrom, ElemTo> mapper
    ) where CollFrom : IEnumerable<ElemFrom> =>
      e.mapRight(mapC<CollFrom, ElemFrom, ElemTo>(mapper));

    [PublicAPI] public static Either<CollTo, Right> mapLeftC<CollFrom, CollTo, Right, ElemFrom, ElemTo>(
      this Either<CollFrom, Right> e,
      Fn<ElemFrom, ElemTo> mapper,
      Fn<IEnumerable<ElemTo>, CollTo> toCollection
    )
      where CollFrom : IEnumerable<ElemFrom>
      where CollTo : IEnumerable<ElemTo>
    => e.mapLeftC(mapper).mapLeft(toCollection);

    [PublicAPI] public static Either<Left, CollTo> mapRightC<CollFrom, CollTo, Left, ElemFrom, ElemTo>(
      this Either<Left, CollFrom> e,
      Fn<ElemFrom, ElemTo> mapper,
      Fn<IEnumerable<ElemTo>, CollTo> toCollection
    )
      where CollFrom : IEnumerable<ElemFrom>
      where CollTo : IEnumerable<ElemTo>
    => e.mapRightC(mapper).mapRight(toCollection);

    [PublicAPI] public static Either<ImmutableList<To>, Right> mapLeftC<From, To, Right>(
      this Either<ImmutableList<From>, Right> e, Fn<From, To> mapper
    ) => mapLeftC(e, mapper, _ => _.ToImmutableList());

    [PublicAPI] public static Either<Left, ImmutableList<To>> mapRightC<From, To, Left>(
      this Either<Left, ImmutableList<From>> e, Fn<From, To> mapper
    ) => mapRightC(e, mapper, _ => _.ToImmutableList());

    [PublicAPI] public static Either<A, ImmutableList<B>> sequence<A, B>(
      this IEnumerable<Either<A, B>> enumerable
    ) {
      // mutable for performance
      var builder = ImmutableList.CreateBuilder<B>();
      foreach (var either in enumerable) {
        if (either.isLeft) return either.__unsafeGetLeft;
        builder.Add(either.__unsafeGetRight);
      }
      return builder.ToImmutable();
    }
  }

  public static class Either {
    [PublicAPI] public static Either<A, B> opt<A, B>(bool condition, A ifFalse, B ifTrue) =>
      condition
      ? Either<A, B>.Right(ifTrue)
      : Either<A, B>.Left(ifFalse);

    [PublicAPI] public static Either<A, B> opt<A, B>(bool condition, Fn<A> ifFalse, B ifTrue) =>
      condition
      ? Either<A, B>.Right(ifTrue)
      : Either<A, B>.Left(ifFalse());

    [PublicAPI] public static Either<A, B> opt<A, B>(bool condition, A ifFalse, Fn<B> ifTrue) =>
      condition
      ? Either<A, B>.Right(ifTrue())
      : Either<A, B>.Left(ifFalse);

    [PublicAPI] public static Either<A, B> opt<A, B>(bool condition, Fn<A> ifFalse, Fn<B> ifTrue) =>
      condition
      ? Either<A, B>.Right(ifTrue())
      : Either<A, B>.Left(ifFalse());
  }

  public
#if ENABLE_IL2CPP
  sealed class
#else
  struct
#endif
  Either<A, B> : IEquatable<Either<A, B>> {
    public static Either<A, B> Left(A value) => new Either<A, B>(value);
    public static Either<A, B> Right(B value) => new Either<A, B>(value);

    readonly A _leftValue;
    readonly B _rightValue;

    public Either(A value) {
      _leftValue = value;
      _rightValue = default;
      isLeft = true;
    }
    public Either(B value) {
      _leftValue = default;
      _rightValue = value;
      isLeft = false;
    }

    #region Equality

    public bool Equals(Either<A, B> other) {
      return isLeft == other.isLeft && (
        (isLeft && Smooth.Collections.EqComparer<A>.Default.Equals(_leftValue, other._leftValue)) ||
        (!isLeft && Smooth.Collections.EqComparer<B>.Default.Equals(_rightValue, other._rightValue))
      );
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Either<A, B> either && Equals(either);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = Smooth.Collections.EqComparer<A>.Default.GetHashCode(_leftValue);
        hashCode = (hashCode * 397) ^ Smooth.Collections.EqComparer<B>.Default.GetHashCode(_rightValue);
        hashCode = (hashCode * 397) ^ isLeft.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Either<A, B> left, Either<A, B> right) { return left.Equals(right); }
    public static bool operator !=(Either<A, B> left, Either<A, B> right) { return !left.Equals(right); }

    #endregion

    [PublicAPI] public readonly bool isLeft;
    [PublicAPI] public bool isRight => ! isLeft;
    [PublicAPI] public Option<A> leftValue => isLeft.opt(_leftValue);
    [PublicAPI] public bool leftValueOut(out A a) {
      a = isLeft ? __unsafeGetLeft : default;
      return isLeft;
    } 
    [PublicAPI] public A __unsafeGetLeft => _leftValue;
    [PublicAPI] public Option<B> rightValue => (! isLeft).opt(_rightValue);
    [PublicAPI] public bool rightValueOut(out B b) {
      b = isRight ? __unsafeGetRight : default;
      return isRight;
    }
    [PublicAPI] public B __unsafeGetRight => _rightValue;

    [PublicAPI] public A leftOrThrow { get {
      if (isLeft) return _leftValue;
      throw new WrongEitherSideException($"Expected to have Left({typeof(A)}), but had {this}.");
    } }

    [PublicAPI] public B rightOrThrow { get {
      if (isRight) return _rightValue;
      throw new WrongEitherSideException($"Expected to have Right({typeof(B)}), but had {this}.");
    } }

    [PublicAPI] public override string ToString() =>
      isLeft ? $"Left({_leftValue})" : $"Right({_rightValue})";

    [PublicAPI] public Either<C, B> flatMapLeft<C>(Fn<A, Either<C, B>> mapper) =>
      isLeft ? mapper(_leftValue) : new Either<C, B>(_rightValue);

    [PublicAPI] public Either<A, C> flatMapRight<C>(Fn<B, Either<A, C>> mapper) =>
      isLeft ? new Either<A, C>(_leftValue) : mapper(_rightValue);

    [PublicAPI] public Either<A, C1> flatMapRight<C, C1>(Fn<B, Either<A, C>> f, Fn<B, C, C1> g) {
      var self = this;
      return isLeft
        ? new Either<A, C1>(_leftValue)
        : f(_rightValue).mapRight(c => g(self._rightValue, c));
    }

    [PublicAPI] public Either<AA, BB> map<AA, BB>(Fn<A, AA> leftMapper, Fn<B, BB> rightMapper) =>
      isLeft
        ? new Either<AA, BB>(leftMapper(_leftValue))
        : new Either<AA, BB>(rightMapper(_rightValue));

    [PublicAPI] public Either<C, B> mapLeft<C>(Fn<A, C> mapper) =>
      isLeft ? new Either<C, B>(mapper(_leftValue)) : new Either<C, B>(_rightValue);

    [PublicAPI] public Either<A, C> mapRight<C>(Fn<B, C> mapper) =>
      isLeft ? new Either<A, C>(_leftValue) : new Either<A, C>(mapper(_rightValue));

    [PublicAPI] public B getOrElse(B onLeft) =>
      isLeft ? onLeft : __unsafeGetRight;

    [PublicAPI] public B getOrElse(Fn<B> onLeft) =>
      isLeft ? onLeft() : __unsafeGetRight;

    [PublicAPI] public C fold<C>(Fn<A, C> onLeft, Fn<B, C> onRight) =>
      isLeft ? onLeft(_leftValue) : onRight(_rightValue);

    [PublicAPI] public void voidFold(Act<A> onLeft, Act<B> onRight)
      { if (isLeft) onLeft(_leftValue); else onRight(_rightValue); }

    [PublicAPI] public Try<B> toTry(Fn<A, Exception> onLeft) =>
      isLeft ? new Try<B>(onLeft(_leftValue)) : new Try<B>(_rightValue);

    [PublicAPI] public Either<B, A> swap =>
      isLeft ? new Either<B, A>(_leftValue) : new Either<B, A>(_rightValue);

    /** Change type of left side, throwing exception if this Either is of left side. */
    [PublicAPI] public Either<AA, B> __unsafeCastLeft<AA>() {
      if (isLeft) throw new WrongEitherSideException(
        $"Can't {nameof(__unsafeCastLeft)}, because this is {this}"
      );
      return new Either<AA, B>(_rightValue);
    }

    /** Change type of right side, throwing exception if this Either is of right side. */
    [PublicAPI] public Either<A, BB> __unsafeCastRight<BB>() {
      if (isRight) throw new WrongEitherSideException(
        $"Can't {nameof(__unsafeCastRight)}, because this is {this}"
      );
      return new Either<A, BB>(_leftValue);
    }

    [PublicAPI] public Option<B> getOrLog(
      string errorMessage, UnityEngine.Object context = null, ILog log = null
    ) {
      if (isLeft) {
        log = log ?? Log.@default;
        log.error($"{errorMessage}: {__unsafeGetLeft}", context);
      }
      return rightValue;
    }

    [PublicAPI] public EitherEnumerator<A, B> GetEnumerator() => new EitherEnumerator<A, B>(this);

    // Converstions from values.
    [PublicAPI] public static implicit operator Either<A, B>(A left) => new Either<A, B>(left);
    [PublicAPI] public static implicit operator Either<A, B>(B right) => new Either<A, B>(right);
  }

  public struct EitherEnumerator<A, B> {
    public readonly Either<A, B> either;
    bool read;

    public EitherEnumerator(Either<A, B> either) : this() { this.either = either; }

    public bool MoveNext() => either.isRight && !read;

    public void Reset() { read = false; }

    public B Current { get {
      read = true;
      return either.rightOrThrow;
    } }
  }

  public class WrongEitherSideException : Exception {
    public WrongEitherSideException(string message) : base(message) {}
  }

  public static class EitherBuilderExts {
    public static LeftEitherBuilder<A> left<A>(this A value) => new LeftEitherBuilder<A>(value);
    public static RightEitherBuilder<B> right<B>(this B value) => new RightEitherBuilder<B>(value);
  }

  public struct LeftEitherBuilder<A> {
    public readonly A leftValue;
    public LeftEitherBuilder(A leftValue) { this.leftValue = leftValue; }
    public Either<A, B> r<B>() => new Either<A, B>(leftValue);
  }

  public struct RightEitherBuilder<B> {
    public readonly B rightValue;
    public RightEitherBuilder(B rightValue) { this.rightValue = rightValue; }
    public Either<A, B> l<A>() => new Either<A, B>(rightValue);
  }
}
