using System;
using pzd.lib.typeclasses;
using JetBrains.Annotations;
using pzd.lib.config;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.serialization;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
  public struct Duration : IStr, IEquatable<Duration> {
    [NonSerialized]
    public static readonly Duration zero = new Duration(0);

    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
    [SerializeField] int _millis;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    public int millis => _millis;

    public static Duration fromSeconds(int seconds) => new Duration(seconds * 1000);
    public static Duration fromSeconds(float seconds) => new Duration(Mathf.RoundToInt(seconds * 1000));

    public Duration(int millis) { _millis = millis; }
    public Duration(TimeSpan timeSpan) : this((int) timeSpan.TotalMilliseconds) {}

    #region Equality

    public bool Equals(Duration other) => millis == other.millis;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Duration duration && Equals(duration);
    }

    public override int GetHashCode() => millis.GetHashCode();

    public static bool operator ==(Duration left, Duration right) { return left.Equals(right); }
    public static bool operator !=(Duration left, Duration right) { return !left.Equals(right); }

    #endregion

    public float seconds => millis / 1000f;
    public int secondsInt => Mathf.RoundToInt(seconds);

    public static Duration operator +(Duration d1, Duration d2) =>
      new Duration(d1.millis + d2.millis);
    public static Duration operator -(Duration d1, Duration d2) =>
      new Duration(d1.millis - d2.millis);
    public static Duration operator *(Duration d, int multiplier) =>
      new Duration(d.millis * multiplier);
    public static Duration operator *(Duration d, float multiplier) =>
      new Duration((int) (d.millis * multiplier));
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

    public TimeSpan toTimeSpan => new TimeSpan(millis * TimeSpan.TicksPerMillisecond);
    public static implicit operator TimeSpan(Duration d) => d.toTimeSpan;
    public static implicit operator Duration(TimeSpan ts) => new Duration(ts);

    public string toMinSecString() => ((int)seconds).toMinSecString();

    public override string ToString() => $"{nameof(Duration)}({millis}ms)";
    public string asString() => $"{millis}ms";

    [NonSerialized]
    public static readonly Numeric<Duration> numeric = new Numeric();
    class Numeric : Numeric<Duration> {
      public Duration add(Duration a1, Duration a2) => a1 + a2;
      public Duration subtract(Duration a1, Duration a2) => a1 - a2;
      public Duration mult(Duration a1, Duration a2) => a1 * a2.millis;
      public Duration div(Duration a1, Duration a2) => a1 / a2.millis;
      public Duration fromInt(int i) => new Duration(i);
      public bool Equals(Duration a1, Duration a2) => a1.Equals(a2);
      public int GetHashCode(Duration obj) => obj.GetHashCode();
      public CompareResult compare(Duration a1, Duration a2) => comparable.compare(a1, a2);
      public int Compare(Duration x, Duration y) => compare(x, y).asInt();
    }

    [NonSerialized]
    public static readonly Comparable<Duration> comparable =
      Comparable.long_.comap((Duration d) => d.millis);

    [NonSerialized]
    public static readonly ISerializedRW<Duration> serializedRW =
      SerializedRW.integer.mapNoFail(l => new Duration(l), d => d.millis);

    [NonSerialized]
    public static readonly Config.Parser<object, Duration> configParser =
      Config.intParser.map(ms => new Duration(ms));

    [NonSerialized] public static readonly Config.Parser<object, Option<Duration>> configOptParser =
      Config.opt(configParser);
  }

  public static class DurationExts {
    [PublicAPI] public static Duration toDuration(this TimeSpan ts) =>
      new Duration((int) ts.TotalMilliseconds);
  }
}