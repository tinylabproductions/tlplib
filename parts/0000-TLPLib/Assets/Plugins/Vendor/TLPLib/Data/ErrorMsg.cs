using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  public struct ErrorMsg : IEquatable<ErrorMsg> {
    public readonly string s;
    public readonly Option<Object> context;

    public ErrorMsg(string s, Object context = default(Object)) {
      this.s = s;
      this.context = context.opt();
    }

    public static implicit operator LogEntry(ErrorMsg errorMsg) => new LogEntry(
      errorMsg.s, 
      ImmutableArray<Tpl<string, string>>.Empty, 
      ImmutableArray<Tpl<string, string>>.Empty, 
      context: errorMsg.context
    );

    #region Equality

    public bool Equals(ErrorMsg other) {
      return string.Equals(s, other.s);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ErrorMsg && Equals((ErrorMsg) obj);
    }

    public override int GetHashCode() {
      return (s != null ? s.GetHashCode() : 0);
    }

    public static bool operator ==(ErrorMsg left, ErrorMsg right) { return left.Equals(right); }
    public static bool operator !=(ErrorMsg left, ErrorMsg right) { return !left.Equals(right); }

    #endregion

    public override string ToString() => $"{nameof(ErrorMsg)}({s})";
  }
}