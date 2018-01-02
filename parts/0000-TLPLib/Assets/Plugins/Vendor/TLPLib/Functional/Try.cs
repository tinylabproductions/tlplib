using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Logger;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Functional {
  public
#if ENABLE_IL2CPP
	class
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
      _value = default(A);
      _exception = ex;
    }

    public bool isSuccess => _exception == null;
    public bool isError => _exception != null;

    public Option<A> value => isSuccess ? F.some(_value) : F.none<A>();
    public Option<A> toOption => value;
    public Option<Exception> exception => isSuccess ? F.none<Exception>() : F.some(_exception);

    public A getOrThrow => isSuccess ? _value : F.throws<A>(_exception);
    public A __unsafeGet => _value;
    public Exception __unsafeException => _exception;

    public A getOrElse(A a) => isSuccess ? _value : a;
    public A getOrElse(Fn<A> a) => isSuccess ? _value : a();

    public Either<Exception, A> toEither =>
      isSuccess ? Either<Exception, A>.Right(_value) : Either<Exception, A>.Left(_exception);

    public Either<string, A> toEitherStr =>
      isSuccess ? Either<string, A>.Right(_value) : Either<string, A>.Left(_exception.ToString());

    public Either<ImmutableList<string>, A> toValidation =>
      isSuccess 
      ? Either<ImmutableList<string>, A>.Right(_value) 
      : Either<ImmutableList<string>, A>.Left(ImmutableList.Create(_exception.Message));
    
    public B fold<B>(B onValue, Fn<Exception, B> onException) => 
      isSuccess ? onValue : onException(_exception);

    public B fold<B>(Fn<A, B> onValue, Fn<Exception, B> onException) => 
      isSuccess ? onValue(_value) : onException(_exception);

    public void voidFold(Act<A> onValue, Act<Exception> onException) {
      if (isSuccess) onValue(_value); else onException(_exception);
    }

    public Try<B> map<B>(Fn<A, B> onValue) {
      if (isSuccess) {
        try { return new Try<B>(onValue(_value)); }
        catch (Exception e) { return new Try<B>(e); } 
      }
      return new Try<B>(_exception);
    }

    public Try<B> flatMap<B>(Fn<A, Try<B>> onValue) {
      if (isSuccess) {
        try { return onValue(_value); }
        catch (Exception e) { return new Try<B>(e); }
      }
      return new Try<B>(_exception);
    }

    public Try<B1> flatMap<B, B1>(Fn<A, Try<B>> onValue, Fn<A, B, B1> g) {
      if (isSuccess) {
        try {
          var a = _value;
          return onValue(a).map(b => g(a, b));
        }
        catch (Exception e) { return new Try<B1>(e); }
      }
      return new Try<B1>(_exception);
    }

    public Option<A> getOrLog(string errorMessage, Object context = null, ILog log = null) {
      if (isError) {
        log = log ?? Log.@default;
        log.error(errorMessage, _exception, context);
      }
      return value;
    }

    public override string ToString() => 
      isSuccess ? $"Success({_value})" : $"Error({_exception})";
  }

  public static class TryExts {
    public static Try<ImmutableList<A>> sequence<A>(
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
