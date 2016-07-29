using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessorMenuItems : MonoBehaviour {
    [UsedImplicitly, MenuItem("Assets/Code Processor/Compiler Warnings/Enable")]
    static void addPragmas() => enablePragmas(false);

    [UsedImplicitly, MenuItem("Assets/Code Processor/Compiler Warnings/Disable")]
    static void removePragmas() => enablePragmas(true);
    
    static void enablePragmas(bool addPragma) {
      selectedPath.voidFold(
        () => EditorUtility.DisplayDialog(
          "Error",
          "Not a valid path.\nYou shouldn't do this in the project window's file tree, use the right panel.", 
          "OK"
        ),
        rootPath => {
          if (askForConfirmation(addPragma, rootPath)) {
            getFilePaths(rootPath, "*.cs").voidFold(
              err => EditorUtility.DisplayDialog("Error", err, "OK"),
              paths => {
                processFiles(paths, addPragma);
                EditorUtility.DisplayDialog(
                  "Success", $"File processing done. {paths.Count} file(s) processed.", "OK"
                );
                AssetDatabase.Refresh();
              }
            );
          }
        }
      );
    }

    static Option<PathStr> selectedPath => 
      AssetDatabase.GetAssetPath(Selection.activeObject).nonEmptyOpt().map(PathStr.a);

    static void processFiles(IEnumerable<PathStr> paths, bool addPragma) {
      foreach (var path in paths) CodePreprocessor.processFile(path, addPragma);
    }

    static bool askForConfirmation(bool addPragma, string path) {
      var str = addPragma ? "disable" : "enable";
      var accepted = EditorUtility.DisplayDialog(
        "Warning", $"Do you want to {str} warnings in following path?\n{path}", "Yes", "No"
      );
      return accepted;
    }

    public static Either<string, ImmutableList<PathStr>> getFilePaths(PathStr rootPath, string fileExt) {
      if (!Directory.Exists(rootPath))
        return (string.Equals($"*{rootPath.extension}", fileExt)).either(
          $"Not a '*.{fileExt}' file.",
          () => ImmutableList.Create(new PathStr(rootPath.path))
        );
      var paths = 
        Directory.GetFiles(rootPath, fileExt, SearchOption.AllDirectories)
        .Select(PathStr.a).ToImmutableList();
      return (paths.Count > 0).either($"No '*.{fileExt}' files in directory.", () => paths);
    }
  }
}