using System;
using System.IO;
using com.tinylabproductions.TLPLib.Filesystem;
using JetBrains.Annotations;
using UnityEditor.Android;

namespace com.tinylabproductions.TLPLib.Editor {
  [UsedImplicitly]
  public class GradlePostprocessor : IPostGenerateGradleAndroidProject {
    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path) {
      var tlplibGradle = PathStr.a(path) / "tlplib" / "build.gradle";

      editFile(
        tlplibGradle,
        (
          "//java.srcDirs = ['src']",
          "java.srcDirs = ['src']"
        ),
        (
          "\ndependencies {",
          @"
dependencies {
    api files('../libs/unity-classes.jar')
"
        )
      );
    }

    public static void editFile(PathStr path, params (string from, string to)[] replacements) {
      var contents = File.ReadAllText(path);

      void replace(string from, string to) {
        if (!contents.Contains(from)) {
          throw new Exception($"Could not find `{from}` in `{path}`");
        }
        if (
          contents.IndexOf(from, StringComparison.Ordinal) !=
          contents.LastIndexOf(from, StringComparison.Ordinal)
        ) {
          throw new Exception($"Found multiple `{from}` in `{path}`");
        }
        contents = contents.Replace(from, to);
      }

      foreach (var replacement in replacements) {
        replace(replacement.from, replacement.to);
      }
      File.WriteAllText(path, contents);
    }
  }
}
