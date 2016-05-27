using System.Collections.Generic;
using System.Collections.Immutable;

namespace com.tinylabproductions.TLPLib.Functional {
  // Non-generated methods for validation.
  public static class Either_Validation2 {
    public static Either<ImmutableList<A>, ImmutableList<B>> sequence<A, B>(
      this IEnumerable<Either<ImmutableList<A>, B>> eithers
    ) {
      var errors = ImmutableList<A>.Empty;
      var result = ImmutableList<B>.Empty;
      foreach (var either in eithers) {
        foreach (var errs in either.leftValue) errors = errors.AddRange(errs);
        if (errors.IsEmpty) {
          // No point in accumulating result if we have at least one error.
          foreach (var b in either.rightValue) result = result.Add(b);
        }
      }
      return errors.IsEmpty
        ? Either<ImmutableList<A>, ImmutableList<B>>.Right(result)
        : Either<ImmutableList<A>, ImmutableList<B>>.Left(errors);
    }
  }
}
