#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
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
        EditorUtils.userInfo(nameof(modifyAllScenesInProject), "Operation canceled by user", Log.Level.WARN);
      }
      return result;
    }

    public static bool openScenesAndDo(IEnumerable<ScenePath> scenes, Action<Scene> doWithLoadedScene, bool askToSave = true) {
      try {
        if (askToSave && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          return false;
        }
        var scenesToUse = scenes.ToImmutableArray();
        var sceneCount = scenesToUse.Count();
        for (var i = 0; i < sceneCount; i++) {
          var currentPath = scenesToUse[i];
          if (EditorUtility.DisplayCancelableProgressBar(
            nameof(openScenesAndDo),
            $"({i}/{sceneCount}) ...{currentPath.path.trimToRight(40)}",
            i / (float)sceneCount
          )) {
            return false;
          }
          var loadedScene = EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);
          doWithLoadedScene(loadedScene);
        }
        return true;
      }
      catch (Exception e) {
        EditorUtils.userInfo($"Error in {nameof(openScenesAndDo)}", e.ToString(), Log.Level.ERROR);
        throw;
      }
      finally {
        EditorUtility.ClearProgressBar();
      }
    }
  }
}
#endif
