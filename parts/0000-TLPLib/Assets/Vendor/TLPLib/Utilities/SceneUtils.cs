using System.IO;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static partial class SceneUtils {
    [PublicAPI] public static string[] getAllSceneNamesInBuild() {
      var sceneCount = SceneManager.sceneCountInBuildSettings;
      var scenes = new string[sceneCount];
      for(var i = 0; i < sceneCount; i++ ) {
        scenes[i] = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
      }
      return scenes;
    }
  }
}