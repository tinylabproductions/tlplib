using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class EditorSceneManagerUtils {
    public static A withScene<A>(ScenePath scenePath, Fn<Scene, A> f) {
      var isLoaded = SceneManagerUtils.loadedScenes.Any(s => s.path == scenePath);
      var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
      var ret = f(scene);
      if (!isLoaded) SceneManager.UnloadSceneAsync(scene);
      return ret;
    }

    public static B withSceneObject<A, B>(
      this RuntimeSceneRefWithComponent<A> sceneRef, Fn<A, B> f
    ) where A : Component =>
      withScene(sceneRef.scenePath, scene => f(scene.findComponentOnRootGameObjects<A>().rightOrThrow));
  }
}