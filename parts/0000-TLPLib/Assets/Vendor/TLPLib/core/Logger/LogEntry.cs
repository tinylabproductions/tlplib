using System;
using System.Collections.Immutable;
using System.Text;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.data;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.serialization;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Logger {
  [PublicAPI] public struct LogEntry : IStr {
    /// <summary>Message for the log entry.</summary>
    public readonly string message;
    /// <summary>key -> value pairs where values make up a set. Things like
    /// type -> (dog or cat or fish) are a good fit here.</summary>
    public readonly ImmutableArray<Tpl<string, string>> tags;
    /// <summary>
    /// key -> value pairs where values can be anything. Things like
    /// bytesRead -> 322344 are a good fit here.
    /// </summary>
    public readonly ImmutableArray<Tpl<string, string>> extras;
    /// <summary>Object which is related to this entry.</summary>
    public readonly object maybeContext;
    /// <summary>A log entry might have backtrace attached to it.</summary>
    public readonly Backtrace? backtrace;
    /// <summary>Whether this entry should be reported to any error tracking that we have.</summary>
    public readonly bool reportToErrorTracking;

    public LogEntry(
      string message,
      ImmutableArray<Tpl<string, string>> tags,
      ImmutableArray<Tpl<string, string>> extras,
      bool reportToErrorTracking = true,
      Backtrace? backtrace = null,
      object context = null
    ) {
      this.message = message;
      this.tags = tags;
      this.extras = extras;
      this.reportToErrorTracking = reportToErrorTracking;
      this.backtrace = backtrace;
      maybeContext = context;
    }

    public string asString() {
      var sb = new StringBuilder(message);
      if (maybeContext != null) sb.Append($" (ctx={maybeContext})");
      if (tags.nonEmpty()) sb.Append($"\n{nameof(tags)}={tags.mkStringEnumNewLines()}");
      if (extras.nonEmpty()) sb.Append($"\n{nameof(extras)}={extras.mkStringEnumNewLines()}");
      if (backtrace.HasValue) sb.Append($"\n{backtrace.Value}");
      return sb.ToString();
    }

    public override string ToString() => asString();

    public static LogEntry simple(
      string message, bool reportToErrorTracking = true, 
      Backtrace? backtrace = null, object context = null
    ) => new LogEntry(
      message: message, 
      tags: ImmutableArray<Tpl<string, string>>.Empty,
      extras: ImmutableArray<Tpl<string, string>>.Empty,
      reportToErrorTracking: reportToErrorTracking,
      backtrace: backtrace, context: context
    );
    public static implicit operator LogEntry(string s) => simple(s); 

    public static LogEntry tags_(
      string message, ImmutableArray<Tpl<string, string>> tags, bool reportToErrorTracking = true, 
      Backtrace? backtrace = null, object context = null
    ) => new LogEntry(
      message: message, tags: tags, extras: ImmutableArray<Tpl<string, string>>.Empty,
      backtrace: backtrace, context: context, reportToErrorTracking: reportToErrorTracking
    );

    public static LogEntry extras_(
      string message, ImmutableArray<Tpl<string, string>> extras, bool reportToErrorTracking = true, 
      Backtrace? backtrace = null, object context = null
    ) => new LogEntry(
      message: message, tags: ImmutableArray<Tpl<string, string>>.Empty, extras: extras,
      backtrace: backtrace, context: context, reportToErrorTracking: reportToErrorTracking
    );

    public static LogEntry fromException(
      string message, Exception ex, bool reportToErrorTracking = true, object context = null
    ) {
      var sb = new StringBuilder();

      void appendEx(Exception e) {
        sb.Append("[");
        sb.Append(e.GetType());
        sb.Append("] ");
        sb.Append(e.Message);
      }
      
      sb.Append(message);
      sb.Append(": ");
      appendEx(ex);
      var backtraceBuilder = ImmutableList.CreateBuilder<BacktraceElem>();
      foreach (var bt in Backtrace.fromException(ex)) {
        backtraceBuilder.AddRange(bt.elements.a);
      }

      var idx = 0;
      var cause = ex.InnerException;
      while (cause != null) {
        sb.Append("\nCaused by [");
        sb.Append(idx);
        sb.Append("]: ");
        appendEx(cause);
        foreach (var bt in Backtrace.fromException(ex)) {
          backtraceBuilder.Add(new BacktraceElem($"### Backtrace for [{idx}] ###", F.none_));
          backtraceBuilder.AddRange(bt.elements.a);
        }
        cause = cause.InnerException;
        idx++;
      }

      var backtrace = backtraceBuilder.ToImmutable().toNonEmpty().map(_ => new Backtrace(_));
      
      return simple(sb.ToString(), reportToErrorTracking, backtrace.toNullable(), context);
    }

    public LogEntry withMessage(string message) =>
      new LogEntry(message, tags, extras, reportToErrorTracking, backtrace, maybeContext);

    public LogEntry withMessage(Func<string, string> message) =>
      new LogEntry(message(this.message), tags, extras, reportToErrorTracking, backtrace, maybeContext);

    public LogEntry withExtras(ImmutableArray<Tpl<string, string>> extras) =>
      new LogEntry(message, tags, extras, reportToErrorTracking, backtrace, maybeContext);

    public LogEntry withExtras(Func<ImmutableArray<Tpl<string, string>>, ImmutableArray<Tpl<string, string>>> extras) =>
      new LogEntry(message, tags, extras(this.extras), reportToErrorTracking, backtrace, maybeContext);

    public static readonly ISerializedRW<ImmutableArray<Tpl<string, string>>> stringTupleArraySerializedRw =
      SerializedRW.immutableArray(SerializedRW.str.tpl(SerializedRW.str));
  }
}