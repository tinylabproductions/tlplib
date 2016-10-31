using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class EitherExts {
    public static Either<A, B> flatten<A, B>(this Either<A, Either<A, B>> e) => 
      e.flatMapRight(_ => _);

    static Fn<CollFrom, IEnumerable<ElemTo>> mapC<CollFrom, ElemFrom, ElemTo>(
      Fn<ElemFrom, ElemTo> mapper
    ) where CollFrom : IEnumerable<ElemFrom> =>
      c => c.Select(elem => mapper(elem));

    public static Either<IEnumerable<ElemTo>, Right> mapLeftC<CollFrom, Right, ElemFrom, ElemTo>(
      this Either<CollFrom, Right> e,
      Fn<ElemFrom, ElemTo> mapper
    ) where CollFrom : IEnumerable<ElemFrom> =>
      e.mapLeft(mapC<CollFrom, ElemFrom, ElemTo>(mapper));

    public static Either<Left, IEnumerable<ElemTo>> mapRightC<CollFrom, Left, ElemFrom, ElemTo>(
      this Either<Left, CollFrom> e,
      Fn<ElemFrom, ElemTo> mapper
    ) where CollFrom : IEnumerable<ElemFrom> =>
      e.mapRight(mapC<CollFrom, ElemFrom, ElemTo>(mapper));

    public static Either<CollTo, Right> mapLeftC<CollFrom, CollTo, Right, ElemFrom, ElemTo>(
      this Either<CollFrom, Right> e,
      Fn<ElemFrom, ElemTo> mapper,
      Fn<IEnumerable<ElemTo>, CollTo> toCollection
    )
      where CollFrom : IEnumerable<ElemFrom>
      where CollTo : IEnumerable<ElemTo>
    => e.mapLeftC(mapper).mapLeft(toCollection);

    public static Either<Left, CollTo> mapRightC<CollFrom, CollTo, Left, ElemFrom, ElemTo>(
      this Either<Left, CollFrom> e,
      Fn<ElemFrom, ElemTo> mapper,
      Fn<IEnumerable<ElemTo>, CollTo> toCollection
    )
      where CollFrom : IEnumerable<ElemFrom>
      where CollTo : IEnumerable<ElemTo>
    => e.mapRightC(mapper).mapRight(toCollection);

    public static Either<ImmutableList<To>, Right> mapLeftC<From, To, Right>(
      this Either<ImmutableList<From>, Right> e, Fn<From, To> mapper
    ) => mapLeftC(e, mapper, _ => _.ToImmutableList());

    public static Either<Left, ImmutableList<To>> mapRightC<From, To, Left>(
      this Either<Left, ImmutableList<From>> e, Fn<From, To> mapper
    ) => mapRightC(e, mapper, _ => _.ToImmutableList());
  }

  public
#if ENABLE_IL2CPP
  class
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
      _rightValue = default(B);
      isLeft = true;
    }
    public Either(B value) {
      _leftValue = default(A);
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
      return obj is Either<A, B> && Equals((Either<A, B>) obj);
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

    public bool isLeft { get; }
    public bool isRight => ! isLeft;
    public Option<A> leftValue => isLeft.opt(_leftValue);
    public A __unsafeGetLeft => _leftValue;
    public Option<B> rightValue => (! isLeft).opt(_rightValue);
    public B __unsafeGetRight => _rightValue;
    
    public A leftOrThrow { get {
      if (isLeft) return _leftValue;
      throw new WrongEitherSideException($"Expected to have Left({typeof(A)}), but had {this}.");
    } }

    public B rightOrThrow { get {
      if (isRight) return _rightValue;
      throw new WrongEitherSideException($"Expected to have Right({typeof(B)}), but had {this}.");
    } }

    public override string ToString() => 
      isLeft ? $"Left({_leftValue})" : $"Right({_rightValue})";

    public Either<C, B> flatMapLeft<C>(Fn<A, Either<C, B>> mapper) =>
      isLeft ? mapper(_leftValue) : new Either<C, B>(_rightValue);

    public Either<A, C> flatMapRight<C>(Fn<B, Either<A, C>> mapper) =>
      isLeft ? new Either<A, C>(_leftValue) : mapper(_rightValue);

    public Either<AA, BB> map<AA, BB>(Fn<A, AA> leftMapper, Fn<B, BB> rightMapper) =>
      isLeft
        ? new Either<AA, BB>(leftMapper(_leftValue))
        : new Either<AA, BB>(rightMapper(_rightValue));

    public Either<C, B> mapLeft<C>(Fn<A, C> mapper) =>
      isLeft ? new Either<C, B>(mapper(_leftValue)) : new Either<C, B>(_rightValue);

    public Either<A, C> mapRight<C>(Fn<B, C> mapper) =>
      isLeft ? new Either<A, C>(_leftValue) : new Either<A, C>(mapper(_rightValue));

    public C fold<C>(Fn<A, C> onLeft, Fn<B, C> onRight) => 
      isLeft ? onLeft(_leftValue) : onRight(_rightValue);

    public void voidFold(Act<A> onLeft, Act<B> onRight)
      { if (isLeft) onLeft(_leftValue); else onRight(_rightValue); }

    [Obsolete("Use #rightValue")]
    public Option<B> toOpt() => rightValue;

    public Try<B> toTry(Fn<A, Exception> onLeft) =>
      isLeft ? new Try<B>(onLeft(_leftValue)) : new Try<B>(_rightValue);

    public Either<B, A> swap => 
      isLeft ? new Either<B, A>(_leftValue) : new Either<B, A>(_rightValue);

    /** Change type of left side, throwing exception if this Either is of left side. */
    public Either<AA, B> __unsafeCastLeft<AA>() {
      if (isLeft) throw new WrongEitherSideException(
        $"Can't {nameof(__unsafeCastLeft)}, because this is {this}"
      );
      return new Either<AA, B>(_rightValue);
    }

    /** Change type of right side, throwing exception if this Either is of right side. */
    public Either<A, BB> __unsafeCastRight<BB>() {
      if (isRight) throw new WrongEitherSideException(
        $"Can't {nameof(__unsafeCastRight)}, because this is {this}"
      );
      return new Either<A, BB>(_leftValue);
    }

    public EitherEnumerator<A, B> GetEnumerator() => new EitherEnumerator<A, B>(this);
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
