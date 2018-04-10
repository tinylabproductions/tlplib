using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  public struct ErrorMsg : IEquatable<ErrorMsg> {
    public readonly string s;
    public readonly Option<Object> context;

    ErrorMsg(string s, Option<Object> context) {
      this.s = s;
      this.context = context;
    }

    public ErrorMsg(string s, Object context = null) : this(s, context.opt()) {}

    [PublicAPI] public ErrorMsg withMessage(Fn<string, string> f) => 
      new ErrorMsg(f(s), context);

    public static implicit operator LogEntry(ErrorMsg errorMsg) => new LogEntry(
      message: errorMsg.s,
      tags: ImmutableArray<Tpl<string, string>>.Empty,
      extras: ImmutableArray<Tpl<string, string>>.Empty,
      context: errorMsg.context
    );

    #region Equality

    public bool Equals(ErrorMsg other) => string.Equals(s, other.s) && context.Equals(other.context);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ErrorMsg msg && Equals(msg);
    }

    public override int GetHashCode() {
      unchecked {
        return ((s != null ? s.GetHashCode() : 0) * 397) ^ context.GetHashCode();
      }
    }

    public static bool operator ==(ErrorMsg left, ErrorMsg right) => left.Equals(right);
    public static bool operator !=(ErrorMsg left, ErrorMsg right) => !left.Equals(right);

    #endregion

    public override string ToString() => $"{nameof(ErrorMsg)}({s})";

    public LogEntry toLogEntry() => new LogEntry(
      s,
      ImmutableArray<Tpl<string, string>>.Empty,
      ImmutableArray<Tpl<string, string>>.Empty,
      context: context
    );
    
    public ErrorMsg withContext(Object context) => new ErrorMsg(s, context);
  }
}
