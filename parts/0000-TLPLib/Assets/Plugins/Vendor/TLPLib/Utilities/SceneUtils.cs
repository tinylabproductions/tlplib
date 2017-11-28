#if UNITY_EDITOR
using System;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class SceneUtils {
    /// <param name="modifyScene">
    /// (Scene -> bool) You need to return true if scene was modified and needs to be saved
    /// </param>
    /// <returns>true - if operation was not canceled by user</returns>
    public static bool modifyAllScenesInProject(Fn<Scene, bool> modifyScene) {
      var result = _modifyAllScenesInProject(modifyScene);
      if (result) {
        if (Log.d.isInfo()) Log.d.info($"{nameof(modifyAllScenesInProject)} completed successfully");
      }
      else {
        EditorUtils.userInfo(nameof(modifyAllScenesInProject), "Operation canceled by user", Log.Level.WARN);
      }
      return result;
    }

    static bool _modifyAllScenesInProject(Fn<Scene, bool> modifyScene) {
      try {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          return false;
        }
        var allScenePaths = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var total = allScenePaths.Length;
        for (var i = 0; i < total; i++) {
          var currentPath = allScenePaths[i];
          if (EditorUtility.DisplayCancelableProgressBar(
            nameof(modifyAllScenesInProject),
            $"({i}/{total}) ...{currentPath.trimToRight(40)}",
            i / (float)total
          )) {
            return false;
          }
          var loadedScene = EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);
          if (modifyScene(loadedScene)) {
            EditorSceneManager.SaveScene(loadedScene);
          }
        }
        if (Log.d.isInfo()) Log.d.info($"{nameof(modifyAllScenesInProject)} completed successfully");
        return true;
      }
      catch (Exception e) {
        EditorUtils.userInfo($"Error in {nameof(modifyAllScenesInProject)}", e.ToString(), Log.Level.ERROR);
        throw;
      }
      finally {
        EditorUtility.ClearProgressBar();
      }
    }
  }
}
#endif
