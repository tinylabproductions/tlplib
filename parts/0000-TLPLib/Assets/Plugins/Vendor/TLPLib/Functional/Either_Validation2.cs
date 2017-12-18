using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Functional {
  // Non-generated methods for validation.
  public static class Either_Validation2 {
    public static Either<ImmutableList<A>, B> validationSuccess<A, B>(this B b) => 
      Either<ImmutableList<A>, B>.Right(b);

    public static Either<ImmutableList<string>, A> stringValidationSuccess<A>(this A b) => 
      b.validationSuccess<string, A>();

    public static Either<ImmutableList<ErrorMsg>, A> errorMsgValidationSuccess<A>(this A b) => 
      b.validationSuccess<ErrorMsg, A>();

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

    /// <summary>
    /// If any of the eithers are left, collects all left sides. Otherwise returns a right
    /// from all the right sides.
    /// </summary>
    public static Either<ImmutableList<A>, ImmutableList<B>> sequenceValidations<A, B>(
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
