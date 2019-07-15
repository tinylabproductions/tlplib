using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Logger;
using Harmony;
using JetBrains.Annotations;
using pzd.lib.functional;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Functional {
  public
#if ENABLE_IL2CPP
	sealed class
#else
	struct
#endif
	Try<A> {

    readonly A _value;
    readonly Exception _exception;

#if ENABLE_IL2CPP
	public Try() {}
#endif

    public Try(A value) {
      _value = value;
      _exception = null;
    }

    public Try(Exception ex) {
      _value = default;
      _exception = ex;
    }

    [PublicAPI] public bool isSuccess => _exception == null;
    [PublicAPI] public bool isError => _exception != null;

    [PublicAPI] public Option<A> value => isSuccess ? F.some(_value) : F.none<A>();
    [PublicAPI] public bool valueOut(out A v) {
      v = isSuccess ? _value : default;
      return isSuccess;
    }
    [PublicAPI] public Option<A> toOption => value;
    [PublicAPI] public Option<Exception> exception => isSuccess ? F.none<Exception>() : F.some(_exception);
    [PublicAPI] public bool exceptionOut(out Exception e) {
      e = isError ? _exception : default;
      return isError;
    }

    [PublicAPI] public A getOrThrow => isSuccess ? _value : F.throws<A>(_exception);
    [PublicAPI] public A __unsafeGet => _value;
    [PublicAPI] public Exception __unsafeException => _exception;

    [PublicAPI] public A getOrElse(A a) => isSuccess ? _value : a;
    [PublicAPI] public A getOrElse(Func<A> a) => isSuccess ? _value : a();

    [PublicAPI] public Either<Exception, A> toEither =>
      isSuccess ? Either<Exception, A>.Right(_value) : Either<Exception, A>.Left(_exception);

    [PublicAPI] public Either<string, A> toEitherStr =>
      isSuccess ? Either<string, A>.Right(_value) : Either<string, A>.Left(_exception.ToString());

    [PublicAPI] public Either<ImmutableList<string>, A> toValidation =>
      isSuccess
      ? Either<ImmutableList<string>, A>.Right(_value)
      : Either<ImmutableList<string>, A>.Left(ImmutableList.Create(_exception.ToString()));

    [PublicAPI] public B fold<B>(B onValue, Func<Exception, B> onException) =>
      isSuccess ? onValue : onException(_exception);

    [PublicAPI] public B fold<B>(Func<A, B> onValue, Func<Exception, B> onException) =>
      isSuccess ? onValue(_value) : onException(_exception);

    [PublicAPI] public void voidFold(Action<A> onValue, Action<Exception> onException) {
      if (isSuccess) onValue(_value); else onException(_exception);
    }

    [PublicAPI] public Try<B> map<B>(Func<A, B> onValue) {
      if (isSuccess) {
        try { return new Try<B>(onValue(_value)); }
        catch (Exception e) { return new Try<B>(e); }
      }
      return new Try<B>(_exception);
    }

    [PublicAPI] public Try<B> flatMap<B>(Func<A, Try<B>> onValue) {
      if (isSuccess) {
        try { return onValue(_value); }
        catch (Exception e) { return new Try<B>(e); }
      }
      return new Try<B>(_exception);
    }

    [PublicAPI] public Try<B1> flatMap<B, B1>(Func<A, Try<B>> onValue, Func<A, B, B1> g) {
      if (isSuccess) {
        try {
          var a = _value;
          return onValue(a).map(b => g(a, b));
        }
        catch (Exception e) { return new Try<B1>(e); }
      }
      return new Try<B1>(_exception);
    }

    [PublicAPI] public Option<A> getOrLog(string errorMessage, Object context = null, ILog log = null) {
      if (isError) {
        log = log ?? Log.@default;
        log.error(errorMessage, _exception, context);
      }
      return value;
    }

    [PublicAPI] public override string ToString() =>
      isSuccess ? $"Success({_value})" : $"Error({_exception})";
    
    public static implicit operator Try<A>(A a) => new Try<A>(a);
    public static implicit operator Try<A>(Exception ex) => new Try<A>(ex);
  }

  public static class TryExts {
    [PublicAPI] public static Try<ImmutableList<A>> sequence<A>(
      this IEnumerable<Try<A>> enumerable
    ) {
      // mutable for performance
      var b = ImmutableList.CreateBuilder<A>();
      foreach (var t in enumerable) {
        if (t.isError) return F.err<ImmutableList<A>>(t.__unsafeException);
        b.Add(t.__unsafeGet);
      }
      return F.scs(b.ToImmutable());
    }
  }
}
