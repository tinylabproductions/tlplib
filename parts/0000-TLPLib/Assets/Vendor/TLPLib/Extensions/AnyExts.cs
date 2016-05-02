using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class AnyExts {
    public class RequirementFailedError : Exception {
      public RequirementFailedError(string message) : base(message) {}
    }

    public static void require<T>(
      this T any, bool requirement, string message, params object[] args
    ) {
      if (! requirement)
        throw new RequirementFailedError(string.Format(message, args));
    }

    public static void locally(this object any, Act local) { local(); }

    public static T locally<T>(this object any, Fn<T> local) {
      return local();
    }

    public static B mapVal<A, B>(this A any, Fn<A, B> mapper) {
      return mapper(any);
    }

    public static A tap<A>(this A any, Act<A> tapper) {
      tapper(any);
      return any;
    }

    public static Option<A> opt<A>(this A a) where A : class { return F.opt(a); }
    public static Option<A> some<A>(this A a) { return F.some(a); }

    public static CastBuilder<A> cast<A>(this A a) where A : class { return new CastBuilder<A>(a); }
  }

  public struct CastBuilder<From> where From : class {
    public readonly From from;

    public CastBuilder(From @from) { this.@from = @from; }

    public Either<string, To> toE<To>() where To : class, From {
      var to = @from as To;
      return to == null
        ? Either<string, To>.Left($"Can't cast {typeof(From)} to {typeof(To)}")
        : Either<string, To>.Right(to);
    }

    public To to<To>() where To : class, From { return toE<To>().rightOrThrow; }
  }
}
