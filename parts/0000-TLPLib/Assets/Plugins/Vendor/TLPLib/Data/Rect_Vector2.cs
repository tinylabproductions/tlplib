using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Rect_Vector2 : IEquatable<Rect_Vector2> {
    public readonly Vector2 lowerLeft, lowerRight, upperLeft, upperRight;

    public Rect_Vector2(Vector2 lowerLeft, Vector2 lowerRight, Vector2 upperLeft, Vector2 upperRight) {
      this.lowerLeft = lowerLeft;
      this.lowerRight = lowerRight;
      this.upperLeft = upperLeft;
      this.upperRight = upperRight;
    }

    public static Rect_Vector2 operator +(Rect_Vector2 r, Vector2 v) =>
      new Rect_Vector2(r.lowerLeft + v, r.lowerRight + v, r.upperLeft + v, r.upperRight + v);

    #region Equality

    public bool Equals(Rect_Vector2 other) {
      return lowerLeft.Equals(other.lowerLeft) && lowerRight.Equals(other.lowerRight) &&
             upperLeft.Equals(other.upperLeft) && upperRight.Equals(other.upperRight);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Rect_Vector2 && Equals((Rect_Vector2)obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = lowerLeft.GetHashCode();
        hashCode = (hashCode * 397) ^ lowerRight.GetHashCode();
        hashCode = (hashCode * 397) ^ upperLeft.GetHashCode();
        hashCode = (hashCode * 397) ^ upperRight.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Rect_Vector2 left, Rect_Vector2 right) { return left.Equals(right); }
    public static bool operator !=(Rect_Vector2 left, Rect_Vector2 right) { return !left.Equals(right); }

    #endregion

    public override string ToString() =>
      $"{nameof(Rect_Vector2)}[" +
      $"{nameof(lowerLeft)}: {lowerLeft}, " +
      $"{nameof(lowerRight)}: {lowerRight}, " +
      $"{nameof(upperLeft)}: {upperLeft}, " +
      $"{nameof(upperRight)}: {upperRight}" +
      $"]";

    public void DrawGizmos() {
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(lowerLeft, upperLeft);
      Gizmos.DrawLine(upperLeft, upperRight);
      Gizmos.DrawLine(upperRight, lowerRight);
      Gizmos.DrawLine(lowerLeft, lowerRight);
    }
  }
}