using System;
using System.Diagnostics;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
  [AdvancedInspector(true)]
  public class RuntimeSceneRef {
    [SerializeField, DontAllowSceneObject, NotNull, Inspect(nameof(inspect))]
    public Object scene;

    [SerializeField, HideInInspector] string _sceneName, _scenePath;

    // Required for AI
    // ReSharper disable once NotNullMemberIsNotInitialized
    RuntimeSceneRef() {}

    public RuntimeSceneRef(Object scene) {
      this.scene = scene;
      prepareForRuntime();
    }

    public string sceneName { get {
      prepareForRuntime();
      return _sceneName;
    } }

    public string scenePath { get {
      prepareForRuntime();
      return _scenePath;
    } }

    [Conditional("UNITY_EDITOR")]
    public void prepareForRuntime() {
#if UNITY_EDITOR
      if (!AssetDatabase.GetAssetPath(scene).EndsWithFast(".unity")) {
        // ReSharper disable once AssignNullToNotNullAttribute
        scene = null;
        _sceneName = _scenePath = "";
      }
      if (scene != null) {
        _sceneName = scene.name;
        _scenePath = AssetDatabase.GetAssetPath(scene);
      }
#endif
    }

    bool inspect() {
      prepareForRuntime();
      return true;
    }
  }
}
