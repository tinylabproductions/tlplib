using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Timeframe : IEquatable<Timeframe> {
    public readonly DateTime start, end;

    public Timeframe(DateTime start, DateTime end) {
      this.start = start;
      this.end = end;
    }

    public TimeSpan duration { get { return end - start; } }

    #region Equality

    public bool Equals(Timeframe other) {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return start.Equals(other.start) && end.Equals(other.end);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((Timeframe) obj);
    }

    public override int GetHashCode() {
      unchecked { return (start.GetHashCode() * 397) ^ end.GetHashCode(); }
    }

    public static bool operator ==(Timeframe left, Timeframe right) { return Equals(left, right); }
    public static bool operator !=(Timeframe left, Timeframe right) { return !Equals(left, right); }

    sealed class StartEndEqualityComparer : IEqualityComparer<Timeframe> {
      public bool Equals(Timeframe x, Timeframe y) {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.start.Equals(y.start) && x.end.Equals(y.end);
      }

      public int GetHashCode(Timeframe obj) {
        unchecked { return (obj.start.GetHashCode() * 397) ^ obj.end.GetHashCode(); }
      }
    }

    static readonly IEqualityComparer<Timeframe> StartEndComparerInstance = new StartEndEqualityComparer();

    public static IEqualityComparer<Timeframe> startEndComparer {
      get { return StartEndComparerInstance; }
    }

    #endregion

    public override string ToString() 
    { return string.Format("Timeframe[{0} to {1}]", start, end); }
  }
}
