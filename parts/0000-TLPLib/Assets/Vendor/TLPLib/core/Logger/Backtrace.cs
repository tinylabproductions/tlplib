using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Logger {
  public struct Backtrace {
    public readonly NonEmpty<ImmutableList<BacktraceElem>> elements;

    public Backtrace(NonEmpty<ImmutableList<BacktraceElem>> elements) { this.elements = elements; }

    public override string ToString() => $"{nameof(Backtrace)}({elements.a.mkStringEnumNewLines()})";

    #region Parsing

    public static Option<Backtrace> parseUnityBacktrace(string backtrace) =>
      Regex.Split(backtrace, "\n")
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        .Select(BacktraceElem.parseUnityBacktraceLine)
        .ToImmutableList()
        .toNonEmpty().map(_ => new Backtrace(_));

    /// <summary>Creates a backtrace from the caller site.</summary>
    public static Option<Backtrace> generateFromHere(int skipFrames = 0) =>
      // +1 means skip current method (generateFromHere)
      convertFromStacktrace(new StackTrace(skipFrames + 1, true));

    /// <summary>Creates a backtrace from given exception.</summary>
    public static Option<Backtrace> fromException(Exception e) =>
      convertFromStacktrace(new StackTrace(e, true));

    public static Option<Backtrace> convertFromStacktrace(StackTrace trace) =>
      from frames in F.opt(trace.GetFrames())
      from _ in frames.Select(_ => _.toBacktraceElem()).ToImmutableList().toNonEmpty()
      select new Backtrace(_);

    #endregion
  }
}