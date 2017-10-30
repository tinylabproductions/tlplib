using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class EditorSceneManagerUtils {
    public static A withScene<A>(string scenePath, Fn<Scene, A> f) {
      var isLoaded = SceneManagerUtils.loadedScenes.Any(s => s.path == scenePath);
      var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
      var ret = f(scene);
      if (!isLoaded) SceneManager.UnloadSceneAsync(scene);
      return ret;
    }
  }
}