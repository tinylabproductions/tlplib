using System.Collections.Generic;
using System.Collections.Immutable;
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
    public const string PRAG_STR = "#pragma warning disable\n";
    static Option<PathStr> selectedPath => 
      AssetDatabase.GetAssetPath(Selection.activeObject).nonEmptyOpt().map(PathStr.a);

    [UsedImplicitly, MenuItem("Assets/Code Processor/Disable Warnings/Enable")]
    static void addPragmas() => enablePragmas(true);

    [UsedImplicitly, MenuItem("Assets/Code Processor/Disable Warnings/Disable")]
    static void removePragmas() => enablePragmas(false);
    
    static void enablePragmas(bool add) {
      selectedPath.voidFold(
        () => EditorUtility.DisplayDialog("Error", "Not a valid path.", "OK"),
        rootPath => {
          if (askForConfirmation(add, rootPath)) {
            getFilePaths(rootPath, "*.cs").voidFold(
              err => EditorUtility.DisplayDialog("Error", err, "OK"),
              paths => {
                processFiles(paths, add);
                EditorUtility.DisplayDialog("Success", "File processing done.", "OK");
              }
            );
          }
        }
      );
    }

    static void processFiles(IEnumerable<PathStr> paths, bool add) {
      foreach (var path in paths) processFile(path, add);
    }

    public static void processFile(string path, bool add) {
      var text = File.ReadAllText(path);
      var editedText = add ? checkAndWritePragmaInFront(text) : removePragmaFromFront(text);
      File.WriteAllText(path, editedText);
    }

    public static string removePragmaFromFront(string text) {
      return hasPragmaInFront(text) ? text.Remove(0, PRAG_STR.Length) : text;
    }

    public static string checkAndWritePragmaInFront(string text) {
      return !hasPragmaInFront(text) ? $"{PRAG_STR}{text}" : text;
    }

    public static bool hasPragmaInFront(string text) => text.StartsWith(PRAG_STR);

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

    static bool askForConfirmation(bool add, string path) {
      var str = add ? "add" : "remove";
      var accepted = EditorUtility.DisplayDialog(
        "Warning", $"Do you want to {str} disable in following path?\n{path}", "Yes", "No"
      );
      return accepted;
    }
  }
}