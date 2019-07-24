using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateConstructor = GeneratedConstructor.None)]
  public partial class ErrorMsg {
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
      ImmutableArray<Tpl<string, string>>.Empty,
      ImmutableArray<Tpl<string, string>>.Empty,
      context: context,
      reportToErrorTracking: reportToErrorTracking
    );
    
    [PublicAPI] public ErrorMsg withMessage(Func<string, string> f) => 
      new ErrorMsg(f(s), context, reportToErrorTracking);
    
    [PublicAPI] public ErrorMsg withContext(object context) => 
      new ErrorMsg(s, context, reportToErrorTracking);
  }
}
