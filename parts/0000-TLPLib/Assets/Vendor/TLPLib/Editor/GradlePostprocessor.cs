using System;
using System.IO;
using JetBrains.Annotations;
using UnityEditor.Android;

namespace com.tinylabproductions.TLPLib.Editor {
  [UsedImplicitly]
  public class GradlePostprocessor : IPostGenerateGradleAndroidProject {
    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path) {
      var tlplibPath = Path.Combine(path, "tlplib");
      var tlplibGradlePath = Path.Combine(tlplibPath, "build.gradle");
      var contents = File.ReadAllText(tlplibGradlePath);

      void replace(string from, string to) {
        if (!contents.Contains(from)) {
          throw new Exception($"Could not find `{from}` in `{tlplibGradlePath}`");
        }
        if (
          contents.IndexOf(from, StringComparison.Ordinal) !=
          contents.LastIndexOf(from, StringComparison.Ordinal)
        ) {
          throw new Exception($"Found multiple `{from}` in `{tlplibGradlePath}`");
        }
        contents = contents.Replace(from, to);
      }

      replace("//java.srcDirs = ['src']", "java.srcDirs = ['src']");
      replace(
        "\ndependencies {",
        @"
dependencies {
    api files('../libs/unity-classes.jar')
"
        );
      File.WriteAllText(tlplibGradlePath, contents);
    }
  }
}
