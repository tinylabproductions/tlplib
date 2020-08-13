using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using pzd.lib.exts;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.log;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateConstructor = ConstructorFlags.None)]
  public readonly partial struct ErrorMsg {
    [PublicAPI] public readonly string s;
    /// <see cref="LogEntry.reportToErrorTracking"/>
    [PublicAPI] public readonly bool reportToErrorTracking;
    [PublicAPI] public readonly Option<object> context;

    ErrorMsg(string s, Option<object> context, bool reportToErrorTracking) {
      this.s = s;
      this.context = context;
      this.reportToErrorTracking = reportToErrorTracking;
    }

    public ErrorMsg(string s, object context = null, bool reportToErrorTracking = true)
      : this(s, context.opt(), reportToErrorTracking) {}

    public static implicit operator LogEntry(ErrorMsg errorMsg) => errorMsg.toLogEntry();

    public LogEntry toLogEntry() => new LogEntry(
      s,
      ImmutableArray<KeyValuePair<string, string>>.Empty,
      ImmutableArray<KeyValuePair<string, string>>.Empty,
      context: context.getOrNull(),
      reportToErrorTracking: reportToErrorTracking
    );
    
    [PublicAPI] public ErrorMsg withMessage(Func<string, string> f) => 
      new ErrorMsg(f(s), context, reportToErrorTracking);
    
    [PublicAPI] public ErrorMsg withContext(object context) => 
      new ErrorMsg(s, context, reportToErrorTracking);
  }
}
