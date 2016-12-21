using System;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.Cryptography {
  public struct CryptoHash : IStr, IEquatable<CryptoHash> {
    // http://stackoverflow.com/a/33568064/935259
    static readonly MD5 md5 = new MD5CryptoServiceProvider();
    static readonly SHA1 sha1 = new SHA1CryptoServiceProvider();

    public enum Kind : byte { MD5, SHA1 }

    public readonly ImmutableArray<byte> bytes;
    public readonly Kind kind;

    public CryptoHash(ImmutableArray<byte> bytes, Kind kind) {
      this.bytes = bytes;
      this.kind = kind;
    }

    public override string ToString() => $"{nameof(CryptoHash)}[{kind}, {asString()}]";

    #region Equality

    public bool Equals(CryptoHash other) {
      return bytes.Equals(other.bytes) && kind == other.kind;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is CryptoHash && Equals((CryptoHash) obj);
    }

    public override int GetHashCode() {
      unchecked { return (bytes.GetHashCode() * 397) ^ (int) kind; }
    }

    public static bool operator ==(CryptoHash left, CryptoHash right) { return left.Equals(right); }
    public static bool operator !=(CryptoHash left, CryptoHash right) { return !left.Equals(right); }

    #endregion

    public static int stringLength_(Kind kind) {
      switch (kind) {
        case Kind.MD5: return 32;
        case Kind.SHA1: return 40;
      }
      return F.matchErr<int>(nameof(Kind), kind.ToString());
    }

    public int stringLength => stringLength_(kind);

    public string asString() {
      // Convert the encrypted bytes back to a string (base 16)
      var sb = new StringBuilder();

      for (var i = 0; i < bytes.Length; i++) {
        sb.Append(Convert.ToString(bytes[i], 16).PadLeft(2, '0'));
      }

      return sb.ToString().PadLeft(stringLength, '0');
    }
    
    public static CryptoHash calculate(string s, Kind kind, Encoding encoding = null) =>
      calculate((encoding ?? Encoding.UTF8).GetBytes(s), kind);

    public static CryptoHash calculate(byte[] bytes, Kind kind) =>
      new CryptoHash(ImmutableArrayUnsafe.createByMove(hashBytes(kind, bytes)), kind);

    static byte[] hashBytes(Kind kind, byte[] bytes) {
      switch (kind) {
        case Kind.MD5: return md5.ComputeHash(bytes);
        case Kind.SHA1: return sha1.ComputeHash(bytes);
      }
      return F.matchErr<byte[]>(nameof(Kind), kind.ToString());
    }
  }
}