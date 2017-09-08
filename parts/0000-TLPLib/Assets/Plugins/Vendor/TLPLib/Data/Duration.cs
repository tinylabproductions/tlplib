﻿using System;
using com.tinylabproductions.TLPLib.Configuration;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
  public struct Duration : IStr, IEquatable<Duration> {
    [NonSerialized]
    public static readonly Duration zero = new Duration(0);

    #region Unity Serialized Fields

#pragma warning disable 649
    [SerializeField] int _millis;
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
      return obj is Duration && Equals((Duration) obj);
    }

    public override int GetHashCode() => millis.GetHashCode();

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

    public TimeSpan toTimeSpan => new TimeSpan(millis * TimeSpan.TicksPerMillisecond);
    public static implicit operator TimeSpan(Duration d) => d.toTimeSpan;

    public string toMinSecString() => ((int)seconds).toMinSecString();

    public override string ToString() => $"{nameof(Duration)}({millis}ms)";
    public string asString() => $"{millis}ms";

    [NonSerialized]
    public static readonly Numeric<Duration> numeric = new Numeric();
    class Numeric : Numeric<Duration> {
      public Duration add(Duration a1, Duration a2) => a1 + a2;
      public Duration subtract(Duration a1, Duration a2) => a1 - a2;
      public Duration fromInt(int i) => new Duration(i);
    }

    [NonSerialized]
    public static readonly Comparable<Duration> comparable = 
      Comparable.long_.comap((Duration d) => d.millis);

    [NonSerialized]
    public static readonly ISerializedRW<Duration> serializedRW =
      SerializedRW.integer.map(l => new Duration(l).some(), d => d.millis);

    [NonSerialized]
    public static readonly Config.Parser<Duration> configParser =
      Config.intParser.map(ms => new Duration(ms));
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