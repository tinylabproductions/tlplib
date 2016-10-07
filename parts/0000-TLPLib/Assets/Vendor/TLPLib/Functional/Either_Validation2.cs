using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace com.tinylabproductions.TLPLib.Functional {
  // Non-generated methods for validation.
  public static class Either_Validation2 {
    public static Either<ImmutableList<A>, B> validationSuccess<A, B>(this B b) => 
      Either<ImmutableList<A>, B>.Right(b);

    public static Either<ImmutableList<string>, A> stringValidationSuccess<A>(this A b) => 
      b.validationSuccess<string, A>();

    public static Either<ImmutableList<A>, B> asValidationErrors<A, B>(
      this ImmutableList<A> errors, B b
    ) =>
      errors.IsEmpty 
      ? Either<ImmutableList<A>, B>.Right(b) 
      : Either<ImmutableList<A>, B>.Left(errors);

    public static Either<ImmutableList<A>, B> asValidationErrors<A, B>(
      this ImmutableList<A> errors, Fn<B> b
    ) =>
      errors.IsEmpty 
      ? Either<ImmutableList<A>, B>.Right(b()) 
      : Either<ImmutableList<A>, B>.Left(errors);

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
