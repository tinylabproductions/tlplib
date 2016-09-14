using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  // I discovered TimeSpan after this was written...
  public struct Duration : IEquatable<Duration> {
    public readonly int millis;

    public static Duration fromSeconds(int seconds) => new Duration(seconds * 1000);
    public static Duration fromSeconds(float seconds) => new Duration(Mathf.RoundToInt(seconds * 1000));

    public Duration(int millis) { this.millis = millis; }

    #region Equality

    public bool Equals(Duration other) => millis == other.millis;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Duration && Equals((Duration) obj);
    }

    public override int GetHashCode() => millis;

    public static bool operator ==(Duration left, Duration right) { return left.Equals(right); }
    public static bool operator !=(Duration left, Duration right) { return !left.Equals(right); }

    #endregion

    public float seconds => millis / 1000f;

    public static Duration operator +(Duration d1, Duration d2) =>
      new Duration(d1.millis + d2.millis);
    public static Duration operator -(Duration d1, Duration d2) =>
      new Duration(d1.millis - d2.millis);
    public static Duration operator *(Duration d, int multiplier) =>
      new Duration(d.millis * multiplier);
    public static Duration operator /(Duration d, float divider) =>
      new Duration((int) (d.millis / divider));

    public static bool operator <(Duration d1, Duration d2) =>
      d1.millis < d2.millis;
    public static bool operator >(Duration d1, Duration d2) =>
      d1.millis > d2.millis;
    public static bool operator <=(Duration d1, Duration d2) =>
      d1.millis <= d2.millis;
    public static bool operator >=(Duration d1, Duration d2) =>
      d1.millis >= d2.millis;

    public override string ToString() => $"{nameof(Duration)}({millis}ms)";
  }

  public static class DurationExts {
    public static Duration milli(this int v) => v.millis();
    public static Duration millis(this int v) => new Duration(v);

    public static Duration second(this int v) => v.seconds();
    public static Duration second(this float v) => v.seconds();
    public static Duration seconds(this int v) => Duration.fromSeconds(v);
    public static Duration seconds(this float v) => Duration.fromSeconds(v);

    public static Duration minute(this int v) => v.minutes();
    public static Duration minute(this float v) => v.minutes();
    public static Duration minutes(this int v) => Duration.fromSeconds(v * 60);
    public static Duration minutes(this float v) => Duration.fromSeconds(v * 60);

    public static Duration hour(this int v) => v.hours();
    public static Duration hour(this float v) => v.hours();
    public static Duration hours(this int v) => Duration.fromSeconds(v * 3600);
    public static Duration hours(this float v) => Duration.fromSeconds(v * 3600);

    public static Duration toDuration(this TimeSpan ts) =>
      new Duration((int) ts.TotalMilliseconds);
  }
}