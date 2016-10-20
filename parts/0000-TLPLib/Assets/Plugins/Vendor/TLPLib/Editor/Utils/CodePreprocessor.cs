using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class CodePreprocessor {
    public const string PRAG_STR = "#pragma warning disable";
    public const string DIRECTIVES_STR = "#";

    public static void processFile(string path, bool add) {
      var lines = File.ReadAllLines(path).ToImmutableList();
      var editedText = (add ? checkAndWritePragma(lines) : checkAndRemovePragma(lines)).ToArray();
      File.WriteAllLines(path, editedText);
    }

    public static ImmutableList<string> checkAndRemovePragma(ImmutableList<string> lines) => 
      lines.process(
        onNoPragma: _ => lines,
        onNoDirectives: () => lines,
        onPragmaExists: (pragmaLineIdx, _) => lines.RemoveAt(pragmaLineIdx)
      );

    public static ImmutableList<string> checkAndWritePragma(ImmutableList<string> lines) => 
      lines.process(
        onNoPragma: lastDirectiveLineIdx => lines.addPragmaAt(lastDirectiveLineIdx + 1),
        onNoDirectives: () => lines.addPragmaAt(0),
        onPragmaExists: (pragmaLineIdx, lastDirectiveIdx) => 
          pragmaLineIdx == lastDirectiveIdx 
          ? lines 
          : lines.reformatPragmas(pragmaLineIdx, lastDirectiveIdx)
      );

    public static ImmutableList<string> process(
      this ImmutableList<string> lines,
      Fn<int, ImmutableList<string>> onNoPragma,
      Fn<int, int, ImmutableList<string>> onPragmaExists,
      Fn<ImmutableList<string>> onNoDirectives
    ) => 
      getLastDirectiveIndex(lines).fold(
        onNoDirectives,
        lastDirectiveIdx => pragmaLineNumber(lines.GetRange(0, lastDirectiveIdx + 1)).fold(
          () => onNoPragma(lastDirectiveIdx),
          pragmaLineIdx => onPragmaExists(pragmaLineIdx, lastDirectiveIdx)
        )
      );

    static ImmutableList<string> addPragmaAt(this ImmutableList<string> lines, int lineIndex) => 
      lines.Insert(lineIndex, PRAG_STR);

    static ImmutableList<string> reformatPragmas(
      this ImmutableList<string> lines, int currentPragmaLineIdx, int lastDirectiveLineIdx
    ) => 
      lines.addPragmaAt(lastDirectiveLineIdx + 1).RemoveAt(currentPragmaLineIdx);

    public static Option<int> getLastDirectiveIndex(this ImmutableList<string> lines) => 
      lines.indexWhere(line => !line.StartsWith(DIRECTIVES_STR)).flatMap(idx => (idx > 0).opt(idx - 1));

    public static Option<int> pragmaLineNumber(this ImmutableList<string> lines) => 
      lines.indexWhere(line => line.StartsWith(PRAG_STR));
  }
}