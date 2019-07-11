using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  /**
   * Position in screen space. [(0, 0), (Screen.width, Screen.height)]
   **/
  public struct ScreenPosition : IEquatable<ScreenPosition> {
    public readonly Vector2 position;

    public ScreenPosition(Vector2 position) { this.position = position; }

    public static ScreenPosition operator +(ScreenPosition sp, Vector2 v) => new ScreenPosition(sp.position + v);

    #region Equality

    public bool Equals(ScreenPosition other) {
      return position.Equals(other.position);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ScreenPosition && Equals((ScreenPosition)obj);
    }

    public override int GetHashCode() {
      return position.GetHashCode();
    }

    public static bool operator ==(ScreenPosition left, ScreenPosition right) { return left.Equals(right); }
    public static bool operator !=(ScreenPosition left, ScreenPosition right) { return !left.Equals(right); }

    #endregion

    public override string ToString() => $"{nameof(ScreenPosition)}({position})";
  }
}