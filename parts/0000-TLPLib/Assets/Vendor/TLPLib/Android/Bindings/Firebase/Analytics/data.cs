using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Android.Bindings.Firebase.Analytics {
  public struct FirebaseUserId : IEquatable<FirebaseUserId> {
    public readonly string id;

    public FirebaseUserId(string id) { this.id = id; }

    #region Equality

    public bool Equals(FirebaseUserId other) {
      return string.Equals(id, other.id);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is FirebaseUserId && Equals((FirebaseUserId) obj);
    }

    public override int GetHashCode() {
      return (id != null ? id.GetHashCode() : 0);
    }

    public static bool operator ==(FirebaseUserId left, FirebaseUserId right) { return left.Equals(right); }
    public static bool operator !=(FirebaseUserId left, FirebaseUserId right) { return !left.Equals(right); }

    #endregion

    public override string ToString() => $"{nameof(FirebaseUserId)}({id})";

    public static Either<string, FirebaseUserId> create(string id) =>
        id == null ? Either<string, FirebaseUserId>.Left("id can't be null") 
      : id.Length > 36 ? Either<string, FirebaseUserId>.Left(
        $"id length must be <= 36, but was {id.Length}: '{id}'"
      ) 
      : Either<string, FirebaseUserId>.Right(new FirebaseUserId(id));
  }

  public struct FirebaseEvent {
    static readonly ImmutableHashSet<string> reservedEventNames = ImmutableHashSet.Create(
      "app_clear_data",
      "app_uninstall",
      "app_update",
      "error",
      "first_open",
      "in_app_purchase",
      "notification_dismiss",
      "notification_foreground",
      "notification_open",
      "notification_receive",
      "os_update",
      "session_start",
      "user_engagement"
    );

    public readonly string name;
    public readonly IDictionary<string, OneOf<string, long, double>> parameters;

    public FirebaseEvent(
      string name, IDictionary<string, OneOf<string, long, double>> parameters
    ) {
      this.name = name;
      this.parameters = parameters;
    }

    public override string ToString() =>
      $"{nameof(FirebaseEvent)}[{name}, {parameters.asString(newlines: false)}]";

    static ImmutableList<string> validateName(string errorPrefix, int maxLength, string name) {
      var errors = ImmutableList<string>.Empty;
      
      if (name.Length < 1 || name.Length > maxLength) errors = errors.Add(
        $"{errorPrefix} name length must be from 1 to {maxLength} chars, " +
        $"but it was {name.Length} chars long: '{name}'"
      );
      if (!name[0].isAlphabetic()) errors = errors.Add(
        $"{errorPrefix} name must start with alphabetic char: '{name}'"
      );
      if (! name.All(c => c.isAlphaNumeric() || c == '_')) errors = errors.Add(
        $"{errorPrefix} name must only contain alphanumeric chars or underscores: '{name}'"
      );
      if (name.StartsWith("firebase_")) errors = errors.Add(
        $"{errorPrefix} name can't start with 'firebase_': '{name}'"
      );

      return errors;
    }

    public static IDictionary<string, OneOf<string, long, double>> emptyParams =>
      new Dictionary<string, OneOf<string, long, double>>();

    public static OneOf<string, long, double> param(string value) =>
      new OneOf<string, long, double>(value);

    public static OneOf<string, long, double> param(long value) =>
      new OneOf<string, long, double>(value);

    public static OneOf<string, long, double> param(double value) =>
      new OneOf<string, long, double>(value);

    public static Either<ImmutableList<string>, FirebaseEvent> a(
      string name, IDictionary<string, OneOf<string, long, double>> parameters
    ) {
      var errors = ImmutableList<string>.Empty;

      // The name of the event. Should contain 1 to 32 alphanumeric characters or underscores. 
      // The name must start with an alphabetic character. Some event names are reserved. See
      // FirebaseAnalytics.Event for the list of reserved event names. The "firebase_" prefix 
      // is reserved and should not be used. Note that event names are case-sensitive and that
      // logging two events whose names differ only in case will result in two distinct events.
      errors = errors.AddRange(validateName("Event", 32, name));
      if (reservedEventNames.Contains(name)) errors = errors.Add(
        $"Event name is reserved: '{name}'"
      );

      // The event can have up to 25 parameters.
      if (parameters.Count() > 25) {
        var paramsStr = parameters.Select(kv => kv.Key).mkString(", ");
        errors = errors.Add(
          $"Event '{name}' has more than 25 parameters: {paramsStr} ({parameters.Count()})"
        );
      }

      // The map of event parameters. Passing null indicates that the event has no parameters. 
      // Parameter names can be up to 24 characters long and must start with an alphabetic 
      // character and contain only alphanumeric characters and underscores. Only String, 
      // long and double param types are supported. String parameter values can be up to 
      // 36 characters long. The "firebase_" prefix is reserved and should not be used 
      // for parameter names.
      foreach (var kv in parameters) {
        errors = errors.AddRange(validateName("Parameter", 24, kv.Key));
        foreach (var str in kv.Value.aValue) {
          if (str.Length > 36) errors = errors.Add(
            $"Parameter '{kv.Key}' value is too long ({str.Length} > 36): {str}"
          );
        }
      }

      return errors.IsEmpty
        ? Either<ImmutableList<string>, FirebaseEvent>.Right(new FirebaseEvent(name, parameters))
        : Either<ImmutableList<string>, FirebaseEvent>.Left(errors);
    }
  }
}