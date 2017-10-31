using System;
using System.Diagnostics;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    protected RuntimeSceneRef() {}

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

  [Serializable]
  public abstract class RuntimeSceneRef<A> : RuntimeSceneRef where A : MonoBehaviour {
    protected RuntimeSceneRef() { }
    protected RuntimeSceneRef(Object scene) : base(scene) { }

    public Future<A> load(LoadSceneMode loadSceneMode = LoadSceneMode.Single) =>
      SceneWithObjectLoader.load<A>(new ScenePath(scenePath), loadSceneMode).map(e => e.rightOrThrow);
  }
}
