using System;
using AdvancedInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable]
  [AdvancedInspector(true)]
  public class RuntimeSceneRef {
    [SerializeField] public Object scene;

    [SerializeField, HideInInspector] string _sceneName;

    public string sceneName => Application.isEditor ? (scene ? scene.name : "") : _sceneName;

#if UNITY_EDITOR
    public void prepareForRuntime() {
      _sceneName = scene.name;
    }
#endif
  }
}
