using System;
using System.Diagnostics;
using AdvancedInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
  [AdvancedInspector(true)]
  public class RuntimeSceneRef {
    [SerializeField] public Object scene;

    [SerializeField, HideInInspector] string _sceneName;

    public string sceneName { get {
      prepareForRuntime();
      return _sceneName;
    } }

    [Conditional("UNITY_EDITOR")]
    public void prepareForRuntime() {
      if (scene) _sceneName = scene.name;
    }
  }
}
