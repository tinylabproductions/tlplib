using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
  // [AdvancedInspector(true)]
  public class RuntimeSceneRef : OnObjectValidate {
    [SerializeField/*, DontAllowSceneObject, NotNull, Inspect(nameof(inspect))*/]
    public Object scene;

    [SerializeField, ReadOnly] string _sceneName, _scenePath;

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

    [Conditional("UNITY_EDITOR"), Button]
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

    public bool onObjectValidateIsThreadSafe => false;
    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      containingComponent.recordEditorChanges($"{nameof(RuntimeSceneRef)}.{nameof(onObjectValidate)}");
      prepareForRuntime();
      return Enumerable.Empty<ErrorMsg>();
    }

    public override string ToString() => $"{nameof(RuntimeSceneRef)}({_scenePath})";
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

    public override string ToString() =>
      $"{nameof(RuntimeSceneRefWithComponent<A>)}({typeof(A)} @ {scenePath.path})";
  }
}
