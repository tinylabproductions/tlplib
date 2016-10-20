using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Point2D : IEquatable<Point2D> {
    public readonly int x, y;

    public Point2D(int x, int y) {
      this.x = x;
      this.y = y;
    }

    public Point2D copy(int? x = null, int? y = null) {
      return new Point2D(x ?? this.x, y ?? this.y);
    }

    public Vector2 ToVector2() { return new Vector2(x, y); }
    public Vector3 ToVector3() { return new Vector3(x, y); }

    #region Generated code

    public override string ToString() {
      return string.Format("({0},{1})", x, y);
    }

    public bool Equals(Point2D other) {
      return x == other.x && y == other.y;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Point2D && Equals((Point2D) obj);
    }

    public override int GetHashCode() {
      unchecked { return (x * 397) ^ y; }
    }

    public static bool operator ==(Point2D left, Point2D right) {
      return left.Equals(right);
    }

    public static bool operator !=(Point2D left, Point2D right) {
      return !left.Equals(right);
    }

    private sealed class XYEqComparer : IEqualityComparer<Point2D> {
      public bool Equals(Point2D x, Point2D y) {
        return x.x == y.x && x.y == y.y;
      }

      public int GetHashCode(Point2D obj) {
        unchecked {
          return (obj.x * 397) ^ obj.y;
        }
      }
    }

    private static readonly IEqualityComparer<Point2D> XYComparerInstance = new XYEqComparer();

    public static IEqualityComparer<Point2D> xYComparer {
      get { return XYComparerInstance; }
    }

    #endregion
  }
}
