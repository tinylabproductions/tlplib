using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.config;

namespace com.tinylabproductions.TLPLib.Configuration {
  public static class ConfigLookupErrorExts {
    public static LogEntry toLogEntry(
      this ConfigLookupError err, string message, ICollection<Tpl<string, string>> extraExtras = null
    ) {
      if (err.kind == ConfigLookupError.Kind.EXCEPTION) {
        return LogEntry.fromException(nameof(ConfigLookupError), err.exception.__unsafeGet);
      }
      ImmutableArray<Tpl<string, string>> extras;
      var errorExtras = err.extras.Select(kv => F.t(kv.Key, kv.Value));
      if (extraExtras == null) {
        extras = errorExtras.ToImmutableArray();
      }
      else {
        var builder = ImmutableArray.CreateBuilder<Tpl<string, string>>(extraExtras.Count + err.extras.Length);
        builder.AddRange(errorExtras);
        builder.AddRange(extraExtras);
        extras = builder.MoveToImmutable();
      }

      return new LogEntry(
        $"{message}: {err.kind}",
        tags: ImmutableArray<Tpl<string, string>>.Empty,
        extras: extras
      );
    }
  }
}