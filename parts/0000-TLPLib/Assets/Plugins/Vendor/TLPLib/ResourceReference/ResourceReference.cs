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
    static string notFound(string path) => $"Resource not found: {path}";

    public static Either<string, A> load<A>(PathStr loadPath) where A : Object {
      var path = loadPath.toUnityPath();
      var csr = Resources.Load<ResourceReference<A>>(path);
      return csr 
        ? F.right<string, A>(csr.reference) 
        : F.left<string, A>(notFound(path));
    }

    public static A loadOrThrow<A>(PathStr loadPath) where A : Object {
      var loaded = load<A>(loadPath);
      if (loaded.isLeft) throw new Exception(loaded.leftOrThrow);
      return loaded.rightOrThrow;
    }

    public static Tpl<ResourceRequest, Future<Either<string, A>>> loadAsync<A>(
      PathStr loadPath
    ) where A : Object {
      var path = loadPath.toUnityPath();
      var request = Resources.LoadAsync<ResourceReference<A>>(path);
      return F.t(request, Future<Either<string, A>>.async(
        p => ASync.StartCoroutine(waitForLoadCoroutine(request, p, path))
      ));
    }

    /// <summary>
    /// Tries to load a resource asynchronously.
    /// If it fails, it logs an error and returns an unfulfilled future.
    /// Otherwise returns fullfilled future with the requested resource. 
    /// </summary>
    public static Tpl<ResourceRequest, Future<A>> loadAsyncUnsafe<A>(
      PathStr loadPath
    ) where A : Object {
      var loadResult = loadAsync<A>(loadPath);
      var throwOrReturnFtr = loadResult._2.flatMap(errorOrResource => {
        if (errorOrResource.isRight) return Future<A>.successful(errorOrResource.rightOrThrow);
        Log.error(errorOrResource.leftOrThrow);
        return Future<A>.unfulfilled;
      });

      return F.t(loadResult._1, throwOrReturnFtr);
    }

    static IEnumerator waitForLoadCoroutine<A>(
      ResourceRequest request, Promise<Either<string, A>> p, string path
    ) where A : Object {
      yield return request;
      var csr = (ResourceReference<A>) request.asset;
      p.complete(csr
        ? F.right<string, A>(csr.reference)
        : F.left<string, A>(notFound(path))
      );
    }
  }
}