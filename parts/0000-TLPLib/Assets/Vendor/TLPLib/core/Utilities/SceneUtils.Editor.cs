#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static partial class SceneUtils {
    /// <param name="modifyScene">
    /// (Scene -> bool) You need to return true if scene was modified and needs to be saved
    /// </param>
    /// <returns>true - if operation was not canceled by user</returns>
    public static bool modifyAllScenesInProject(Func<Scene, bool> modifyScene) {
      var result = openScenesAndDo(
        AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).Select(_ => new ScenePath(_)),
        scene => { if (modifyScene(scene)) EditorSceneManager.SaveScene(scene); }
      );

      if (result) {
        if (Log.d.isInfo()) Log.d.info($"{nameof(modifyAllScenesInProject)} completed successfully");
      }
      else {
        EditorUtils.userInfo(nameof(modifyAllScenesInProject), "Operation canceled by user", LogLevel.WARN);
      }
      return result;
    }

    public static bool openScenesAndDo(IEnumerable<ScenePath> scenes, Action<Scene> doWithLoadedScene, bool askToSave = true, bool allAtOnce = false) {
      try {
        if (askToSave && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          return false;
        }
        var scenesToUse = scenes.ToImmutableArray();
        var sceneCount = scenesToUse.Count();
        var initialScene = SceneManager.GetActiveScene().scenePath();
        try {
          var loadedScenes = new List<Scene>(sceneCount);
          for (var i = 0; i < sceneCount; i++) {
            var currentPath = scenesToUse[i];
            if (progress(i, currentPath, secondLoop: false)) {
              return false;
            }
            var mode = (!allAtOnce || i == 0) ? OpenSceneMode.Single : OpenSceneMode.Additive;
            var loadedScene = EditorSceneManager.OpenScene(currentPath, mode);
            if (allAtOnce) {
              loadedScenes.Add(loadedScene);
            }
            else {
              doWithLoadedScene(loadedScene);
            }
          }

          if (allAtOnce) {
            for (var i = 0; i < loadedScenes.Count; i++) {
              var scene = loadedScenes[i];
              if (progress(i, scene.scenePath(), secondLoop: true)) {
                return false;
              }
              doWithLoadedScene(scene);
            }
          }
        }
        finally {
          // unload all scenes and load previously opened scene
          if (initialScene.path.isNullOrEmpty()) {
            // if path is empty, that means empty scene was loaded before
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
          }
          else {
            EditorSceneManager.OpenScene(initialScene, OpenSceneMode.Single);
          }
        }

        bool progress(int sceneNumber, ScenePath path, bool secondLoop) {
          var progressValue = sceneNumber / (float) sceneCount;
          if (allAtOnce) {
            progressValue /= 2;
            if (secondLoop) progressValue += .5f;
          }
          return EditorUtility.DisplayCancelableProgressBar(
            nameof(openScenesAndDo),
            $"({sceneNumber}/{sceneCount}) ...{path.path.trimToRight(40)}",
            progressValue
          );
        }
        return true;
      }
      catch (Exception e) {
        EditorUtils.userInfo($"Error in {nameof(openScenesAndDo)}", e.ToString(), LogLevel.ERROR);
        throw;
      }
      finally {
        EditorUtility.ClearProgressBar();
      }
    }
  }
}
#endif
