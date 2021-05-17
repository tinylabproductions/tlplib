using System.Collections.Generic;
using System.Linq;
using pzd.lib.collection;
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
      var extras = extraExtras == null ? err.extras : err.extras.Concat(extraExtras).toImmutableArrayC();

      return new LogEntry($"{message}: {err.kind}", extras: extras);
    }
  }
}