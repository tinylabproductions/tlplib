using System;
using System.Diagnostics;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Utilities;
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
  public class RuntimeSceneRef : OnObjectValidate {
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

    public SceneName sceneName { get {
      prepareForRuntime();
      return new SceneName(_sceneName);
    } }

    public ScenePath scenePath { get {
      prepareForRuntime();
      return new ScenePath(_scenePath);
    } }

    [Conditional("UNITY_EDITOR")]
    public void prepareForRuntime() {
#if UNITY_EDITOR
      var previousPath = _scenePath;
      if (!AssetDatabase.GetAssetPath(scene).EndsWithFast(".unity")) {
        // ReSharper disable once AssignNullToNotNullAttribute
        scene = null;
        _sceneName = _scenePath = "";
      }
      if (scene != null) {
        _sceneName = scene.name;
        _scenePath = AssetDatabase.GetAssetPath(scene);
      }
      if (previousPath != _scenePath) Log.d.warn($"Scene name : {_sceneName}");
#endif
    }

    bool inspect() {
      prepareForRuntime();
      return true;
    }

    public void onObjectValidate(Object containingComponent) {
      containingComponent.recordEditorChanges($"{nameof(RuntimeSceneRef)}.{nameof(onObjectValidate)}");
      prepareForRuntime();
    }
  }

  /// <summary>
  /// Reference to a <see cref="Scene"/> which has a <see cref="Component"/> of type <see cref="A"/> on
  /// a root <see cref="GameObject"/> in it.
  /// </summary>
  [Serializable]
  public abstract class RuntimeSceneRefWithComponent<A> : RuntimeSceneRef where A : Component {
    protected RuntimeSceneRefWithComponent() { }
    protected RuntimeSceneRefWithComponent(Object scene) : base(scene) { }

    public Future<A> load(LoadSceneMode loadSceneMode = LoadSceneMode.Single) =>
      SceneWithObjectLoader.load<A>(scenePath, loadSceneMode).map(e => e.rightOrThrow);
  }
}
