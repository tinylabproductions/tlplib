using System;
using System.Diagnostics;
#if ADVANCED_INSPECTOR
using AdvancedInspector;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
#if ADVANCED_INSPECTOR
  [AdvancedInspector(true)]
#endif
  public class RuntimeFolderRef {
#if ADVANCED_INSPECTOR
    [SerializeField, DontAllowSceneObject, Inspect(nameof(inspect))]
#else
    [SerializeField]
#endif
    public Object folder;

    [SerializeField, HideInInspector] string _folderName;
    
    public string folderName { get {
      prepareForRuntime();
      return _folderName;
    } }

    [Conditional("UNITY_EDITOR")]
    public void prepareForRuntime() {
#if UNITY_EDITOR
      if (folder) _folderName = AssetDatabase.GetAssetPath(folder);
      if (!AssetDatabase.IsValidFolder(_folderName)) {
        // ReSharper disable once AssignNullToNotNullAttribute
        folder = null;
        _folderName = "";
      }
#endif
    }

    bool inspect() {
      prepareForRuntime();
      return true;
    }
  }
}
