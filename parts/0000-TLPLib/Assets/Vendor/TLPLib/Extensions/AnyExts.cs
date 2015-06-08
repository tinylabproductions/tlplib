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
  }
}
