using System;
using System.Text;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class BiMapper {
    public static BiMapper<A, B> a<A, B>(Fn<A, B> map, Fn<B, A> comap) =>
      new BiMapper<A, B>(map, comap);

    public static readonly Encoding defaultEncoding = Encoding.UTF8;

    public static readonly BiMapper<byte[], string> utf8ByteArrString = 
      byteArrString(Encoding.UTF8);

    public static BiMapper<byte[], string> byteArrString(Encoding encoding = null) {
      encoding = encoding ?? defaultEncoding;
      return a<byte[], string>(encoding.GetString, encoding.GetBytes);
    }
  }

  public struct BiMapper<A, B> : IEquatable<BiMapper<A, B>> {
    public readonly Fn<A, B> map;
    public readonly Fn<B, A> comap;

    public BiMapper(Fn<A, B> map, Fn<B, A> comap) {
      this.map = map;
      this.comap = comap;
    }

    public BiMapper<B, A> reverse => new BiMapper<B, A>(comap, map);

    #region Equality

    public bool Equals(BiMapper<A, B> other) {
      return Equals(map, other.map) && Equals(comap, other.comap);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is BiMapper<A, B> && Equals((BiMapper<A, B>) obj);
    }

    public override int GetHashCode() {
      unchecked { return ((map != null ? map.GetHashCode() : 0) * 397) ^ (comap != null ? comap.GetHashCode() : 0); }
    }

    public static bool operator ==(BiMapper<A, B> left, BiMapper<A, B> right) { return left.Equals(right); }
    public static bool operator !=(BiMapper<A, B> left, BiMapper<A, B> right) { return !left.Equals(right); }

    #endregion
  }
}