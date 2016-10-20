using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Assertions {
  public class RequirementFailedError : Exception {
    public RequirementFailedError(string message) : base(message) {}
  }

  public static class AssertionExts {
    public static void require<T>(
      this T any, bool requirement, string message, params object[] args
    ) {
      if (! requirement)
        throw new RequirementFailedError(string.Format(message, args));
    }

    public static void requireNotNullOrEmpty(this string s, string message=null) {
      if (string.IsNullOrEmpty(s)) {
        throw new RequirementFailedError(
          message ??
          $"Expected string to not be null or empty, but it was " + (s == null ? "null" : "empty")
        );
      }
    }

    public static void requireNotEmpty<A>(this ICollection<A> a, string message=null) {
      if (a.Count == 0) {
        throw new RequirementFailedError(
          message ?? $"Expected {a.GetType()} to not be empty, but it was!"
        );
      }
    }

    public static void requireNotNull<A>(this A a, string message = null) where A : class {
      if (a == null) {
        throw new RequirementFailedError(
          message ?? $"Expected {typeof(A)} to not be null, but it was!"
        );
      }
    }

    public static Option<string> requireExistsOpt(this UnityEngine.Object a, string message = null) => 
      (!a).opt(() => message ?? $"Expected unity object to exist, but it did not!");

    public static void requireExists(this UnityEngine.Object a, string message = null) {
      foreach (var err in requireExistsOpt(a, message)) 
        throw new RequirementFailedError(err);
    }
  }
}
