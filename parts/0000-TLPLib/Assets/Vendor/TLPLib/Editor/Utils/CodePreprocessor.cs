using System;
using System.Collections.Immutable;
using UnityEngine;
using System.IO;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public class CodePreprocessor : MonoBehaviour {
    public const string PRAG_STR = "#pragma warning disable\n";
    static Option<string> selectedPath => AssetDatabase.GetAssetPath(Selection.activeObject).nonEmptyOpt();

    [UsedImplicitly, MenuItem("Assets/Code processor/Disable Warnings/Enable")]
    static void addPragmas() => enablePragmas(true);

    [UsedImplicitly, MenuItem("Assets/Code processor/Disable Warnings/Disable")]
    static void removePragmas() => enablePragmas(false);

    static void enablePragmas(bool add) {
      var rootPath = selectedPath;
      if (!showDialog(add, rootPath.get))
        return;
      if (rootPath.isEmpty) {
        EditorUtility.DisplayDialog("Error", "Not a valid path.", "OK");
        return;
      }
      var paths = getFilePaths(rootPath.get, "*.cs");
      if (paths.isEmpty)
      {
        EditorUtility.DisplayDialog("Error", "No '*.cs' files selected.", "OK");
        return;
      }
      foreach (var path in paths.get) {
        processFile(path, add);
      }
      EditorUtility.DisplayDialog("Success", "File processing done.", "OK");
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

    public static Option<ImmutableArray<string>> getFilePaths(string rootPath, string fileExt) {
      if (Directory.Exists(rootPath)) {
        var paths = Directory.GetFiles(rootPath, fileExt, SearchOption.AllDirectories).ToImmutableArray();
        return paths.Length > 0 ? new Option<ImmutableArray<string>>(paths) : new Option<ImmutableArray<string>>();
      }
      if (!string.Equals($"*{Path.GetExtension(rootPath)}",fileExt)) return new Option<ImmutableArray<string>>();
      var pth = ImmutableArray.Create(rootPath);
      return new Option<ImmutableArray<string>>(pth);
    }

    static bool showDialog(bool add, string path) {
      var str = add ? "add" : "remove";
      var accepted = EditorUtility.DisplayDialog(
        "Warning", $"Do you want to {str} disable in following path?\n{path}", "Yes", "No"
      );
      return accepted;
    }
  }
}