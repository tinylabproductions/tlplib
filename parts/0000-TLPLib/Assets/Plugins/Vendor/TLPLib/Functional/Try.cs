using System;

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

    public Either<Exception, A> toEither =>
      isSuccess ? Either<Exception, A>.Right(_value) : Either<Exception, A>.Left(_exception);

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

    public override string ToString() => 
      isSuccess ? $"Success({_value})" : $"Error({_exception})";
  }
}
