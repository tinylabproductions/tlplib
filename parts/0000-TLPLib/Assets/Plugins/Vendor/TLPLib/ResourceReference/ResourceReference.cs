using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace com.tinylabproductions.TLPLib.ResourceReference {
  public abstract class ResourceReference<A> : ScriptableObject where A : Object {
#pragma warning disable 649
    [SerializeField, NotNull] A _reference;
#pragma warning restore 649

    public A reference => _reference;

#if UNITY_EDITOR
    public A editorReference {
      get { return _reference; }
      set { _reference = value; }
    }
#endif
  }

  public static class ResourceReference {
#if UNITY_EDITOR
    public static SO create<SO, A>(string path, A reference) 
      where SO : ResourceReference<A> where A : Object
    {
      var so = ScriptableObject.CreateInstance<SO>();
      so.editorReference = reference;
      AssetDatabase.CreateAsset(so, path);
      return so;
    }
#endif

    public static Option<A> load<A>(PathStr loadPath) where A : Object {
      var path = loadPath.toUnityPath();
      var csr = Resources.Load<ResourceReference<A>>(path);
      return csr ? csr.reference.some() : Option<A>.None;
    }

    public static Tpl<ResourceRequest, Future<Option<A>>> loadAsync<A>(
      PathStr loadPath
    ) where A : Object {
      var path = loadPath.toUnityPath();
      var request = Resources.LoadAsync<ResourceReference<A>>(path);
      return F.t(request, Future<Option<A>>.async(
        p => ASync.StartCoroutine(waitForLoadCoroutine(request, p))
      ));
    }

    static IEnumerator waitForLoadCoroutine<A>(
      ResourceRequest request, Promise<Option<A>> p
    ) where A : Object {
      yield return request;
      var csr = (ResourceReference<A>) request.asset;
      p.complete(csr ? csr.reference.some() : Option<A>.None);
    }
  }
}