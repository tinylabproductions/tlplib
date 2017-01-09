using System;
using com.tinylabproductions.TLPLib.Data.typeclasses;

namespace com.tinylabproductions.TLPLib.Data {
  /** Stupid tag on string. Because System.Uri is heavy. */
  public struct Url : IStr, IEquatable<Url> {
    public readonly string url;

    public Url(string url) { this.url = url; }
    public static Url a(string url) => new Url(url);

    public override string ToString() => $"{nameof(Url)}({url})";

    #region Equality

    public bool Equals(Url other) {
      return string.Equals(url, other.url);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Url && Equals((Url) obj);
    }

    public override int GetHashCode() {
      return (url != null ? url.GetHashCode() : 0);
    }

    public static bool operator ==(Url left, Url right) { return left.Equals(right); }
    public static bool operator !=(Url left, Url right) { return !left.Equals(right); }

    #endregion

    public string asString() => url;

    public static implicit operator string(Url url) => url.asString();

    public static Url operator +(Url u1, Url u2) => new Url(u1.url + u2.url);
  }

  public static class UrlExts {
    public static Url toUrl(this Uri uri) => new Url(uri.ToString());
  }
}