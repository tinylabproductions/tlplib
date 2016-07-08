using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using System.IO;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessor : MonoBehaviour {
    public const string PRAG_STR = "#pragma warning disable";
    public const string DIRECTIVES_STR = "#";

    static Option<PathStr> selectedPath => 
      AssetDatabase.GetAssetPath(Selection.activeObject).nonEmptyOpt().map(PathStr.a);

    [UsedImplicitly, MenuItem("Assets/Code Processor/Compiler Warnings/Enable")]
    static void addPragmas() => enablePragmas(false);

    [UsedImplicitly, MenuItem("Assets/Code Processor/Compiler Warnings/Disable")]
    static void removePragmas() => enablePragmas(true);
    
    static void enablePragmas(bool addPragma) {
      selectedPath.voidFold(
        () => EditorUtility.DisplayDialog("Error",
                  "Not a valid path. \nYou shouldn't do this in the project window's file tree, use the right panel.", "OK"),
        rootPath => {
          if (askForConfirmation(addPragma, rootPath)) {
            getFilePaths(rootPath, "*.cs").voidFold(
              err => EditorUtility.DisplayDialog("Error", err, "OK"),
              paths => {
                processFiles(paths, addPragma);
                EditorUtility.DisplayDialog(
                  "Success", $"File processing done. {paths.Length} file(s) processed.", "OK"
                );
              }
            );
          }
        }
      );
    }

    static void processFiles(IEnumerable<PathStr> paths, bool addPragma) {
      foreach (var path in paths) processFile(path, addPragma);
    }

    public static void processFile(string path, bool add) {
      var lines = File.ReadAllLines(path).ToImmutableArray();
      var editedText = (add ? checkAndWritePragma(lines) : checkAndRemovePragma(lines)).ToArray();
      File.WriteAllLines(path, editedText);
    }

    public static ImmutableArray<string> checkAndRemovePragma(ImmutableArray<string> lines) {
      var lastIndex = getLastDirectiveIndex(lines);
      foreach (var last in lastIndex) {
        var pragLine = pragmaLineNumber(lines, last);
        foreach (var prag in pragLine) {
          return removePragma(lines, prag);
        }
        return lines;
      }
      return lines;
    }

    public static ImmutableArray<string> checkAndWritePragma(ImmutableArray<string> lines) {
      var lastIndex = getLastDirectiveIndex(lines);
      foreach (var last in lastIndex) { 
        var pragLine = pragmaLineNumber(lines, last);
        foreach (var prag in pragLine) {
          return prag == last ? lines : reformatPragmas(lines, prag, last);
        }
        return addPragma(lines, last + 1);
      }
      return addPragma(lines, 0);
    }


    static ImmutableArray<string> removePragma(ImmutableArray<string> lines, int pragmaIndex) {
      return lines.RemoveAt(pragmaIndex);
    } 
    
    static ImmutableArray<string> addPragma(ImmutableArray<string> lines, int lineIndex) {
      return lines.Insert(lineIndex, PRAG_STR);
    }

    static ImmutableArray<string> reformatPragmas(ImmutableArray<string> lines, int pragIndex, int lastIndex) {
      var linesNew = addPragma(lines, lastIndex + 1);
      return linesNew.RemoveAt(pragIndex);
    }

    public static Option<int> getLastDirectiveIndex(ImmutableArray<string> lines) {
      var i = 0;
      while (lines[i].StartsWith(DIRECTIVES_STR)) {
        i++;
      }
      return (i != 0) ? F.some(i-1) : F.none<int>();
    }

    public static Option<int> pragmaLineNumber(ImmutableArray<string> lines, int lastIndex) {
      for (var i = 0; i < lastIndex + 1; i++) {
        if (lines[i].StartsWith(PRAG_STR)) {
          return F.some(i);
        }
      }
      return F.none<int>();
    }

    public static Either<string, ImmutableArray<PathStr>> getFilePaths(PathStr rootPath, string fileExt) {
      if (!Directory.Exists(rootPath))
        return (string.Equals($"*{rootPath.extension}", fileExt)).either(
          "Not a '*.cs' file.",
          () => ImmutableArray.Create(new PathStr(rootPath.path))
        );
      var paths = 
        Directory.GetFiles(rootPath, fileExt, SearchOption.AllDirectories)
        .Select(PathStr.a).ToImmutableArray();
      return (paths.Length > 0).either("No '*.cs' files in directory.", () => paths);
    }

    static bool askForConfirmation(bool addPragma, string path) {
      var str = addPragma ? "disable" : "enable";
      var accepted = EditorUtility.DisplayDialog(
        "Warning", $"Do you want to {str} warnings in following path?\n{path}", "Yes", "No"
      );
      return accepted;
    }
  }
}