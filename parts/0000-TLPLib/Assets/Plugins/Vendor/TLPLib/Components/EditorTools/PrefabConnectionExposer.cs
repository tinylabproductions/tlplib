using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.EditorTools {
  /// <summary>
  /// Allows you to expose connections from instances to prefab objects in editor and do things
  /// with them, like applying properties from instance to the prefab.
  /// </summary>
  public class PrefabConnectionExposer : MonoBehaviour {
    public GameObject instance { get; private set; }
    public GameObject prefab { get; private set; }

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void expose(GameObject instance, GameObject prefab) {
      var exposer = instance.AddComponent<PrefabConnectionExposer>();
      exposer.instance = instance;
      exposer.prefab = prefab;
    }

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void expose(Component instance, Component prefab) =>
      expose(instance.gameObject, prefab.gameObject);

    [Conditional("UNITY_EDITOR"), PublicAPI]
    public static void expose<A>(
      IEnumerable<A> instances, IEnumerable<A> prefabs,
      Func<A, IEnumerable<GameObject>> navigateCollection
    ) {
      var zipped = instances.SelectMany(navigateCollection).zip(prefabs.SelectMany(navigateCollection));
      foreach (var t in zipped) {
        var (instance, prefab) = t;
        expose(instance, prefab);
      }
    }
  }
}