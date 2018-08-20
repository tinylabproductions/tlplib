using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateConstructor = GeneratedConstructor.None)]
  public partial struct ErrorMsg {
    [PublicAPI] public readonly string s;
    /// <see cref="LogEntry.reportToErrorTracking"/>
    [PublicAPI] public readonly bool reportToErrorTracking;
    [PublicAPI] public readonly Option<Object> context;

    ErrorMsg(string s, Option<Object> context, bool reportToErrorTracking) {
      this.s = s;
      this.context = context;
      this.reportToErrorTracking = reportToErrorTracking;
    }

    public ErrorMsg(string s, Object context = null, bool reportToErrorTracking = true)
      : this(s, context.opt(), reportToErrorTracking) {}

    public static implicit operator LogEntry(ErrorMsg errorMsg) => errorMsg.toLogEntry();

    public LogEntry toLogEntry() => new LogEntry(
      s,
      ImmutableArray<Tpl<string, string>>.Empty,
      ImmutableArray<Tpl<string, string>>.Empty,
      context: context,
      reportToErrorTracking: reportToErrorTracking
    );
    
    [PublicAPI] public ErrorMsg withMessage(Fn<string, string> f) => 
      new ErrorMsg(f(s), context, reportToErrorTracking);
    
    [PublicAPI] public ErrorMsg withContext(Object context) => 
      new ErrorMsg(s, context, reportToErrorTracking);
  }
}
