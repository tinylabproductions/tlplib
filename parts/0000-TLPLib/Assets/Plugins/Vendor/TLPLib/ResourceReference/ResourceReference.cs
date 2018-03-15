using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  /// <summary>
  /// A thing you save in to resources folder that allows you to reference something outside
  /// of resources folder.
  /// </summary>
  public abstract partial class ResourceReference<A> : ScriptableObject where A : Object {
#pragma warning disable 649
    [SerializeField, NotNull, PublicAccessor] A _reference;
#pragma warning restore 649

#if UNITY_EDITOR
    public A editorReference {
      set => _reference = value;
    }
#endif
  }

  public static class ResourceReference {
#if UNITY_EDITOR
    [PublicAPI]
    public static SO create<SO, A>(string path, A reference)
      where SO : ResourceReference<A> where A : Object
    {
      var so = ScriptableObject.CreateInstance<SO>();
      so.editorReference = reference;
      UnityEditor.AssetDatabase.CreateAsset(so, path);
      return so;
    }
#endif

    static string notFound(string path) => $"Resource not found: {path}";

    [PublicAPI]
    public static Either<string, A> load<A>(PathStr loadPath) where A : Object {
      var path = loadPath.unityPath;
      var csr = Resources.Load<ResourceReference<A>>(path);
      return csr
        ? F.right<string, A>(csr.reference)
        : F.left<string, A>(notFound(path));
    }

    [PublicAPI]
    public static Tpl<ResourceRequest, Future<Either<ErrorMsg, A>>> loadAsync<A>(
      PathStr loadPath
    ) where A : Object {
      var path = loadPath.unityPath;
      var request = Resources.LoadAsync<ResourceReference<A>>(path);
      return F.t(request, Future<Either<ErrorMsg, A>>.async(
        p => ASync.StartCoroutine(waitForLoadCoroutine<A>(request, p.complete, path))
      ));
    }

    [PublicAPI]
    public static Tpl<ResourceRequest, Future<A>> loadAsyncIgnoreErrors<A>(
      PathStr loadPath, bool logOnError = true
    ) where A : Object =>
      loadAsync<A>(loadPath).map2(future => future.dropError(logOnError));

    [PublicAPI]
    public static IEnumerator waitForLoadCoroutine<A>(
      ResourceRequest request, Action<Either<ErrorMsg, A>> whenDone, string path
    ) where A : Object {
      yield return request;
      var csr = (ResourceReference<A>) request.asset;
      whenDone(
        csr
        ? F.right<ErrorMsg, A>(csr.reference)
        : F.left<ErrorMsg, A>(new ErrorMsg(notFound(path)))
      );
    }
  }
}
