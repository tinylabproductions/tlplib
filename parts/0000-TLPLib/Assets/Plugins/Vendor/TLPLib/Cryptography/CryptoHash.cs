using System;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;

namespace com.tinylabproductions.Cryptography {
  public struct CryptoHash : IStr, IEquatable<CryptoHash> {
    // http://stackoverflow.com/a/33568064/935259
    static readonly MD5 md5 = new MD5CryptoServiceProvider();
    static readonly SHA1 sha1 = new SHA1CryptoServiceProvider();
    // If this doesn't compile change 'Api Compatibility Level' to '.NET 2.0' in Player Settings
    static readonly SHA256 sha256 = new SHA256CryptoServiceProvider();

    public enum Kind : byte { MD5, SHA1, SHA256 }

    [PublicAPI] public readonly ImmutableArray<byte> bytes;
    [PublicAPI] public readonly Kind kind;

    public CryptoHash(ImmutableArray<byte> bytes, Kind kind) {
      this.bytes = bytes;
      this.kind = kind;
    }

    public override string ToString() => $"{nameof(CryptoHash)}[{kind}, {asString()}]";

    #region Equality

    public bool Equals(CryptoHash other) => bytes.Equals(other.bytes) && kind == other.kind;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is CryptoHash hash && Equals(hash);
    }

    public override int GetHashCode() {
      unchecked { return (bytes.GetHashCode() * 397) ^ (int) kind; }
    }

    public static bool operator ==(CryptoHash left, CryptoHash right) => left.Equals(right);
    public static bool operator !=(CryptoHash left, CryptoHash right) => !left.Equals(right);

    #endregion

    public static int stringLength_(Kind kind) {
      switch (kind) {
        case Kind.MD5: return 32;
        case Kind.SHA1: return 40;
        case Kind.SHA256: return 64;
        default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
      }
    }

    public int stringLength => stringLength_(kind);

    public string asString() => bytes.internalArray().asHexString().PadLeft(stringLength, '0');

    public static CryptoHash calculate(string s, Kind kind, Encoding encoding = null) =>
      calculate((encoding ?? Encoding.UTF8).GetBytes(s), kind);

    public static CryptoHash calculate(byte[] bytes, Kind kind) =>
      new CryptoHash(ImmutableArrayUnsafe.createByMove(hashBytes(kind, bytes)), kind);

    static byte[] hashBytes(Kind kind, byte[] bytes) {
      switch (kind) {
        case Kind.MD5: return md5.ComputeHash(bytes);
        case Kind.SHA1: return sha1.ComputeHash(bytes);
        case Kind.SHA256: return sha256.ComputeHash(bytes);
        default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
      }
    }
  }
}