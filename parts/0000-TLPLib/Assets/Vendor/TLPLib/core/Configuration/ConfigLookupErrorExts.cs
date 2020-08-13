using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.config;
using pzd.lib.log;

namespace com.tinylabproductions.TLPLib.Configuration {
  public static class ConfigLookupErrorExts {
    public static LogEntry toLogEntry(
      this ConfigLookupError err, string message, ICollection<KeyValuePair<string, string>> extraExtras = null
    ) {
      if (err.kind == ConfigLookupError.Kind.EXCEPTION) {
        return LogEntry.fromException(nameof(ConfigLookupError), err.exception.__unsafeGet);
      }
      ImmutableArray<KeyValuePair<string, string>> extras;
      var errorExtras = err.extras.Select(kv => F.kv(kv.Key, kv.Value));
      if (extraExtras == null) {
        extras = errorExtras.ToImmutableArray();
      }
      else {
        var builder = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>(extraExtras.Count + err.extras.Length);
        builder.AddRange(errorExtras);
        builder.AddRange(extraExtras);
        extras = builder.MoveToImmutable();
      }

      return new LogEntry(
        $"{message}: {err.kind}",
        tags: ImmutableArray<KeyValuePair<string, string>>.Empty,
        extras: extras
      );
    }
  }
}