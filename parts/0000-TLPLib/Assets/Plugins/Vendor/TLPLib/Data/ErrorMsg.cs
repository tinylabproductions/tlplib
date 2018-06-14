using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateConstructor = false)]
  public partial struct ErrorMsg {
    [PublicAPI] public readonly string s;
    /// <see cref="LogEntry.reportToErrorTracking"/>
    [PublicAPI] public readonly bool reportToErrorTracking;
    [PublicAPI] public readonly Option<Object> context;

    ErrorMsg(string s, bool reportToErrorTracking, Option<Object> context) {
      this.s = s;
      this.reportToErrorTracking = reportToErrorTracking;
      this.context = context;
    }

    public ErrorMsg(string s, bool reportToErrorTracking = true, Object context = null) 
      : this(s, reportToErrorTracking, context.opt()) {}

    public static implicit operator LogEntry(ErrorMsg errorMsg) => errorMsg.toLogEntry();

    public LogEntry toLogEntry() => new LogEntry(
      s,
      ImmutableArray<Tpl<string, string>>.Empty,
      ImmutableArray<Tpl<string, string>>.Empty,
      context: context
    );
    
    [PublicAPI] public ErrorMsg withMessage(Fn<string, string> f) => 
      new ErrorMsg(f(s), reportToErrorTracking, context);
    [PublicAPI] public ErrorMsg withContext(Object context) => new ErrorMsg(s, context);
  }
}
