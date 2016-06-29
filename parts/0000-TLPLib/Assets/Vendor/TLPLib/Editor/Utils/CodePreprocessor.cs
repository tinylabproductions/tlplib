using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.IO;
using Assets.Vendor.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Utils {


  public class CodePreprocessor : MonoBehaviour {

    public const string PRAG_STR = "#pragma warning disable\r\n";


    [MenuItem("Assets/Code processor/Add warning disable")]
    static void EnablePragmas() {
      EnablePragmas(true);
    }

    [MenuItem("Assets/Code processor/Remove warning disable")]
    static void RemovePragmas() {
      EnablePragmas(false);
    }


    static void EnablePragmas(bool add) {
      var rootPath = GetSelectedPath();
      if (!ShowDialog(add, rootPath.get))
        return;
      if (rootPath.isEmpty) {
        Debug.LogError("Not a valid path.");
        return;
      }
      var paths = GetFilePaths(rootPath.get);
      if (paths.isEmpty) {
        Debug.LogError("No files '*.cs' files selected");
        return;
      }
      foreach (var path in paths.get) {
        ProcessFile(path, add);
      }
      Debug.Log("File processing done");
    }
    
    static void ProcessFile(string path, bool add) {
      var text = File.ReadAllText(path);
      var editedText = add ? CheckAndWritePragmaInFront(text) : RemovePragmaFromFront(text);
      File.WriteAllText(path, editedText);
    }


    public static string RemovePragmaFromFront(string text) {
      return HasPragmaInFront(text) ? text.Remove(0, PRAG_STR.Length) : text;
    }

    public static string CheckAndWritePragmaInFront(string text) {
      return !HasPragmaInFront(text) ? PRAG_STR + text : text;
    }

    public static bool HasPragmaInFront(string text)
    {
      var front = text.Substring(0, PRAG_STR.Length);
      return (front.Contains(PRAG_STR));
    }


    static bool IsDirectory(string path)
    {
      var attr = File.GetAttributes(path);
      return (attr == FileAttributes.Directory);
    }

    static Option<ImmutableArray<string>> GetFilePaths(string rootPath)
    {
      if (IsDirectory(rootPath))
      {
        var paths = GetFileList("*.cs", rootPath).ToImmutableArray();
        return paths.Length > 0 ? new Option<ImmutableArray<string>>(paths) : new Option<ImmutableArray<string>>();
      }
      else {
        if (Path.GetExtension(rootPath) == ".cs")
        {
          var pth = ImmutableArray.Create(rootPath);
          return new Option<ImmutableArray<string>>(pth);
        }
        else {
          return new Option<ImmutableArray<string>>();
        }
      }
    }


    static IEnumerable<string> GetFileList(string fileSearchPattern, string rootPath) {
      var pending = new Queue<string>();
      pending.Enqueue(rootPath);
      while (pending.Count > 0) {
        rootPath = pending.Dequeue();
        var tmp = Directory.GetFiles(rootPath, fileSearchPattern);
        foreach (var t in tmp) {
          yield return t;
        }
        tmp = Directory.GetDirectories(rootPath);
        foreach (var t in tmp) {
          pending.Enqueue(t);
        }
      }
    }

    static Option<string> GetSelectedPath() {
      var path = AssetDatabase.GetAssetPath(Selection.activeObject);
      return path == "" ? new Option<string>() : new Option<string>(path);
    }

    static bool ShowDialog(bool add, string path) {
      var str = (add) ? "add" : "remove";
      var accepted = EditorUtility.DisplayDialog(
        "Warning", "Do you want to " + str + " #pragma warning disable in following path?\n" + path, "Yes", "No");
      return accepted;
    }
  }


}