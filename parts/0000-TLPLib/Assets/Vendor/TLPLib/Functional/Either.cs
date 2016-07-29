using System;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class EitherExts {
    public static Either<A, B> flatten<A, B>(this Either<A, Either<A, B>> e)
      { return e.flatMapRight(_ => _); }
  }

  public
#if ENABLE_IL2CPP
  class
#else
  struct
#endif
  Either<A, B> : IEquatable<Either<A, B>> {
    public static Either<A, B> Left(A value) { return new Either<A, B>(value); }
    public static Either<A, B> Right(B value) { return new Either<A, B>(value); }

    private readonly A _leftValue;
    private readonly B _rightValue;
    private readonly bool _isLeft;

    public Either(A value) {
      _leftValue = value;
      _rightValue = default(B);
      _isLeft = true;
    }
    public Either(B value) {
      _leftValue = default(A);
      _rightValue = value;
      _isLeft = false;
    }

    #region Equality

    public bool Equals(Either<A, B> other) {
      return _isLeft == other._isLeft && (
        (_isLeft && Smooth.Collections.EqComparer<A>.Default.Equals(_leftValue, other._leftValue)) ||
        (!_isLeft && Smooth.Collections.EqComparer<B>.Default.Equals(_rightValue, other._rightValue))
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
        hashCode = (hashCode * 397) ^ _isLeft.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Either<A, B> left, Either<A, B> right) { return left.Equals(right); }
    public static bool operator !=(Either<A, B> left, Either<A, B> right) { return !left.Equals(right); }

    #endregion

    public bool isLeft { get { return _isLeft; } }
    public bool isRight { get { return ! _isLeft; } }
    public Option<A> leftValue { get { return isLeft.opt(_leftValue); } }
    public Option<B> rightValue { get { return (! isLeft).opt(_rightValue); } }

    public override string ToString() {
      return isLeft ? "Left(" + _leftValue + ")" : "Right(" + _rightValue + ")";
    }

    public Either<C, B> flatMapLeft<C>(Fn<A, Either<C, B>> mapper)
      { return fold(mapper, F.right<C, B>); }

    public Either<A, C> flatMapRight<C>(Fn<B, Either<A, C>> mapper)
      { return fold(F.left<A, C>, mapper); }

    public Either<AA, BB> map<AA, BB>(Fn<A, AA> leftMapper, Fn<B, BB> rightMapper)
      { return fold(v => F.left<AA, BB>(leftMapper(v)), v => F.right<AA, BB>(rightMapper(v))); }

    public Either<C, B> mapLeft<C>(Fn<A, C> mapper)
      { return fold(v => F.left<C, B>(mapper(v)), F.right<C, B>); }

    public Either<A, C> mapRight<C>(Fn<B, C> mapper)
      { return fold(F.left<A, C>, v => F.right<A, C>(mapper(v))); }

    public C fold<C>(Fn<A, C> onLeft, Fn<B, C> onRight)
      { return isLeft ? onLeft(_leftValue) : onRight(_rightValue); }

    public void voidFold(Act<A> onLeft, Act<B> onRight)
      { if (isLeft) onLeft(_leftValue); else onRight(_rightValue); }

    public Option<B> toOpt() { return rightValue; }

    public Try<B> toTry(Fn<A, Exception> onLeft)
      { return fold(a => F.err<B>(onLeft(a)), F.scs); }

    public A leftOrThrow { get {
      if (isLeft) return _leftValue;
      throw new WrongEitherSideException($"Expected to have Left({typeof(A)}), but had {this}.");
    } }

    public B rightOrThrow { get {
      if (isRight) return _rightValue;
      throw new WrongEitherSideException($"Expected to have Right({typeof(B)}), but had {this}.");
    } }
  }

  public class WrongEitherSideException : Exception {
    public WrongEitherSideException(string message) : base(message) {}
  }

  public static class EitherBuilderExts {
    public static LeftEitherBuilder<A> left<A>(this A value) {
      return new LeftEitherBuilder<A>(value);
    }
    public static RightEitherBuilder<B> right<B>(this B value) {
      return new RightEitherBuilder<B>(value);
    }
  }

  public
#if ENABLE_IL2CPP
    class
#else
    struct
#endif
    LeftEitherBuilder<A> {
    public readonly A leftValue;

    public LeftEitherBuilder(A leftValue) {
      this.leftValue = leftValue;
    }

    public Either<A, B> r<B>() { return new Either<A, B>(leftValue); }
  }

  public
#if ENABLE_IL2CPP
    class
#else
    struct
#endif
    RightEitherBuilder<B> {
    public readonly B rightValue;

    public RightEitherBuilder(B rightValue) {
      this.rightValue = rightValue;
    }

    public Either<A, B> l<A>() { return new Either<A, B>(rightValue); }
  }
}
