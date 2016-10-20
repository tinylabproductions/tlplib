using System;

namespace com.tinylabproductions.TLPLib.Functional.Matching {
  public interface IVoidMatcher<in Base> where Base : class {
    IVoidMatcher<Base> when<T>(Act<T> onMatch) where T : Base;
  }

  public interface IMatcher<in Base, Return> where Base : class {
    IMatcher<Base,Return> when<T>(Fn<T, Return> onMatch)
    where T : Base;

    Return get();
    Return getOrElse(Fn<Return> elseFunc);
  }

  public class MatchError : Exception {
    public MatchError(string message) : base(message) { }
  }

  public
#if ENABLE_IL2CPP
  class
#else
  struct
#endif
  Matcher<Base, Return> 
  : IVoidMatcher<Base>, IMatcher<Base, Return>
  where Base : class {
    private readonly Base subject;

    public Matcher(Base subject) {
      this.subject = subject;
    }

    public IVoidMatcher<Base> when<T>(Act<T> onMatch) where T : Base {
      if (subject is T) {
        onMatch((T) subject);
        return new SuccessfulMatcher<Base, Unit>(F.unit);
      }
      else return this;
    }

    public IMatcher<Base, Return> when<T>(Fn<T, Return> onMatch)
    where T : Base {
      if (subject is T) {
        var casted = (T) subject;
        return new SuccessfulMatcher<Base, Return>(onMatch.Invoke(casted));
      }

      return this;
    }

    public Return get() {
      throw new MatchError(string.Format(
        "Subject {0} of type {1} couldn't be matched!", subject, typeof(Base)
      ));
    }

    public Return getOrElse(Fn<Return> elseFunc) { return elseFunc.Invoke(); }
  }

  public
#if ENABLE_IL2CPP
  class
#else
  struct
#endif
  SuccessfulMatcher<Base, Return> 
  : IVoidMatcher<Base>, IMatcher<Base, Return>
  where Base : class {
    private readonly Return result;

    public SuccessfulMatcher(Return result) {
      this.result = result;
    }

    public IVoidMatcher<Base> when<T>(Act<T> onMatch) 
    where T : Base { return this; }

    public IMatcher<Base, Return> when<T>(Fn<T, Return> onMatch)
    where T : Base { return this; }

    public Return get() { return result; }

    public Return getOrElse(Fn<Return> elseFunc) { return get(); }
  }

  public
#if ENABLE_IL2CPP
  class
#else
  struct
#endif
  MatcherBuilder<T> where T : class {
    private readonly T subject;

    public MatcherBuilder(T subject) {
      this.subject = subject;
    }

    public IMatcher<T, Return> returning<Return>() {
      return new Matcher<T, Return>(subject);
    }
  }

  public static class Match {
    public static MatcherBuilder<T> match<T>(this T subject)
    where T : class { return new MatcherBuilder<T>(subject); }

    public static IVoidMatcher<T> matchVoid<T>(this T subject)
    where T : class { return new Matcher<T, Unit>(subject); }
  }
}
