using JetBrains.Annotations;
using UnityEditor;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  class EditorAssetsUtils {
    [UsedImplicitly, MenuItem("TLP/Tools/Reserialize all assets")]
    static void reserializeAllAssets() {
      if (!EditorUtility.DisplayDialog("Slow operation", "Do you really want to reserialize all assets?", "Yes", "No"))
        return;
      using (var editorProgress = new EditorProgress("Reserializing All Assets")) { 
        var assetsPaths = editorProgress.execute("Loading all assets", AssetDatabase.GetAllAssetPaths);
        
        editorProgress.execute("Setting assets dirty", () => {
          for (var i = 0; i < assetsPaths.Length; i++) {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetsPaths[i]);
            var isCanceled = editorProgress.progressCancellable(i, assetsPaths.Length);
            if (isCanceled) break;
            if (asset != null) EditorUtility.SetDirty(asset);
          }
        });
        
        editorProgress.execute("Saving reserialized assets", AssetDatabase.SaveAssets);
      }
    }
  }
}
