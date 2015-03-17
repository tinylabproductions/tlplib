using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ScreenUtils {
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

      sealed class WidthHeightEqualityComparer : IEqualityComparer<Size> {
        public bool Equals(Size x, Size y) {
          return x.width == y.width && x.height == y.height;
        }

        public int GetHashCode(Size obj) {
          unchecked { return (obj.width * 397) ^ obj.height; }
        }
      }

      static readonly IEqualityComparer<Size> WidthHeightComparerInstance = new WidthHeightEqualityComparer();

      public static IEqualityComparer<Size> widthHeightComparer {
        get { return WidthHeightComparerInstance; }
      }

      #endregion

      public override string ToString() { return string.Format("ScreenSize[width: {0}, height: {1}]", width, height); }
    }

    private static float sw { get { return Screen.width; } }
    private static float sh { get { return Screen.height; } }

    /** Convert screen width percentage to absolute value. **/
    public static float pWidthToAbs(this float percentWidth) {
      return sw * percentWidth;
    }

    /** Convert screen height percentage to absolute value. **/
    public static float pHeightToAbs(this float percentHeight) {
      return sh * percentHeight;
    }

    /** Convert screen width absolute value to percentage. **/
    public static float aWidthToPerc(this float absoluteWidth) {
      return absoluteWidth / sw;
    }

    /** Convert screen height absolute value to percentage. **/
    public static float aHeightToPerc(this float absoluteHeight) {
      return absoluteHeight / sh;
    }
  }
}
