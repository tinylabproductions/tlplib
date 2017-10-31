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

    public static A retrieveComponent<A>(this RuntimeSceneRef<A> sceneRef) where A : MonoBehaviour =>
      withScene(
        new ScenePath(sceneRef.scenePath), 
        scene => 
          scene.GetRootGameObjects()
          .collectFirst(go => go.GetComponent<A>().opt())
          .__unsafeGetValue
      );
  }
}