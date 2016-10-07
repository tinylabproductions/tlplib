using System;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Size : IEquatable<Size> {
    public readonly int width, height;

    public Size(int width, int height) {
      this.width = width;
      this.height = height;
    }

    #region Equality

    public bool Equals(Size other) {
      return width == other.width && height == other.height;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Size && Equals((Size) obj);
    }

    public override int GetHashCode() {
      unchecked { return (width * 397) ^ height; }
    }

    public static bool operator ==(Size left, Size right) { return left.Equals(right); }
    public static bool operator !=(Size left, Size right) { return !left.Equals(right); }

    #endregion

    public override string ToString() { return $"{nameof(Size)}[{width}x{height}]"; }
  }
}
