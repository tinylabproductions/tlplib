using System;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional.Matching {
  public struct Matcher<Base, Return> where Base : class {
    readonly Base subject;
    readonly Option<Return> result;

    public Matcher(Base subject) {
      this.subject = subject;
      result = F.none<Return>();
    }

    Matcher(Return result) {
      subject = default(Base);
      this.result = result.some();
    }

    static Fn<A, Return> actToFn<A>(Act<A> act) { return a => { act(a); return default(Return); }; }

    #region Constrained matchers

    public Matcher<Base, Return> doWhen<A>(Act<A> onMatch) where A : class, Base 
    { return when(actToFn(onMatch)); }

    public Matcher<Base, Return> when<A>(Fn<A, Return> onMatch) where A : class, Base 
    { return when(_ => true, onMatch); }

    public Matcher<Base, Return> doWhen<A>(Fn<A, bool> guard, Act<A> onMatch) where A : class, Base 
    { return when(guard, actToFn(onMatch)); }

    public Matcher<Base, Return> when<A>(Fn<A, bool> guard, Fn<A, Return> onMatch) where A : class, Base 
    { return whenU(guard, onMatch); }

    #endregion

    #region Unconstrained matchers

    public Matcher<Base, Return> doWhenU<A>(Act<A> onMatch) where A : class 
    { return whenU(actToFn(onMatch)); }

    public Matcher<Base, Return> whenU<A>(Fn<A, Return> onMatch) where A : class 
    { return whenU(_ => true, onMatch); }

    public Matcher<Base, Return> doWhenU<A>(Fn<A, bool> guard, Act<A> onMatch) where A : class 
    { return whenU(guard, actToFn(onMatch)); }

    public Matcher<Base, Return> whenU<A>(Fn<A, bool> guard, Fn<A, Return> onMatch) where A : class {
      if (result.isDefined) return this;

      if (subject is A) {
        var casted = (A)(object)subject;
        if (guard(casted)) return new Matcher<Base, Return>(onMatch(casted));
      }

      return this;
    }

    #endregion

    #region Getters

    public void orElse(Act<Base> act) { if (result.isEmpty) act(subject); }
    public Return get() {
      var s = subject;
      return result.getOrThrow(() => new MatchError(string.Format(
        "Subject {0} of type {1} couldn't be matched!", s, typeof(Base)
      )));
    }
    public Return getOrElse(Fn<Return> elseFunc) { return result.getOrElse(elseFunc); }
    public Return getOrElse(Return elseVal) { return result.getOrElse(elseVal); }

    #endregion
  }

  public class MatchError : Exception {
    public MatchError(string message) : base(message) { }
  }

  public struct MatcherBuilder<T> where T : class {
    private readonly T subject;

    public MatcherBuilder(T subject) {
      this.subject = subject;
    }

    public Matcher<T, Return> returning<Return>() {
      return new Matcher<T, Return>(subject);
    }
  }

  public static class Match {
    public static MatcherBuilder<T> match<T>(this T subject)
    where T : class { return new MatcherBuilder<T>(subject); }

    public static Matcher<T, Unit> matchVoid<T>(this T subject)
    where T : class { return new Matcher<T, Unit>(subject); }
  }
}
