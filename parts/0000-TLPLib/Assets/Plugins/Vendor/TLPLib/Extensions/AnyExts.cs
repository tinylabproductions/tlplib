using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class AnyExts {
    public static void locally(this object any, Action local) { local(); }

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

    public static string asString<A>(this A a) {
      var enumerable = a as IEnumerable;
      // ReSharper disable once InvokeAsExtensionMethod
      return enumerable != null 
        ? IEnumerableExts.asString(enumerable) 
        : a == null ? "null" : a.ToString();
    }
  }

  public struct CastBuilder<From> where From : class {
    public readonly From from;

    public CastBuilder(From from) { this.from = from; }

    public Either<string, To> toE<To>() where To : class, From {
      var to = from as To;
      return to == null
        ? Either<string, To>.Left(errorMsg<To>())
        : Either<string, To>.Right(to);
    }

    string errorMsg<To>() => $"Can't cast {from.GetType()} to {typeof(To)}";

    public To to<To>() where To : class, From {
      var to = from as To;
      if (to == null) throw new InvalidCastException(errorMsg<To>());
      return to;
    }
  }
}
