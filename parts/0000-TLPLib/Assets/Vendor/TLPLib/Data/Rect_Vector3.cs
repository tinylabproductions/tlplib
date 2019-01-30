using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Rect_Vector3 : IEquatable<Rect_Vector3> {
    public readonly Vector3 lowerLeft, lowerRight, upperLeft, upperRight;

    public Rect_Vector3(Vector3 lowerLeft, Vector3 lowerRight, Vector3 upperLeft, Vector3 upperRight) {
      this.lowerLeft = lowerLeft;
      this.lowerRight = lowerRight;
      this.upperLeft = upperLeft;
      this.upperRight = upperRight;
    }

    public static Rect_Vector3 operator +(Rect_Vector3 r, Vector3 v) =>
      new Rect_Vector3(r.lowerLeft + v, r.lowerRight + v, r.upperLeft + v, r.upperRight + v);

    public static implicit operator Rect_Vector2(Rect_Vector3 r) =>
      new Rect_Vector2(r.lowerLeft, r.lowerRight, r.upperLeft, r.upperRight);

    #region Equality

    public bool Equals(Rect_Vector3 other) {
      return lowerLeft.Equals(other.lowerLeft) && lowerRight.Equals(other.lowerRight) &&
             upperLeft.Equals(other.upperLeft) && upperRight.Equals(other.upperRight);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Rect_Vector3 && Equals((Rect_Vector3)obj);
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

    public static bool operator ==(Rect_Vector3 left, Rect_Vector3 right) { return left.Equals(right); }
    public static bool operator !=(Rect_Vector3 left, Rect_Vector3 right) { return !left.Equals(right); }

    #endregion

    public override string ToString() =>
      $"{nameof(Rect_Vector3)}[" +
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

    public Rect_Vector3 map(Fn<Vector3, Vector3> transformPoint) =>
      new Rect_Vector3(
        transformPoint(lowerLeft),
        transformPoint(lowerRight),
        transformPoint(upperLeft),
        transformPoint(upperRight)
      );
  }
}