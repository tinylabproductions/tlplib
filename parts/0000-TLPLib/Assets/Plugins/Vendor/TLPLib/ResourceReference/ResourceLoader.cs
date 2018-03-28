using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public static class ResourceLoader {
    static string notFound<A>(string path) => $"Resource of type {typeof(A).FullName} not found at: {path}";
    
    [PublicAPI]
    public static Either<string, A> load<A>(PathStr loadPath) where A : Object {
      var path = loadPath.unityPath;
      var a = Resources.Load<A>(path);
      if (a) return a;
      return notFound<A>(path);
    }

    public static Tpl<ResourceRequest, Future<Either<string, A>>> loadAsync<A>(
      PathStr loadPath
    ) where A : Object {
      var path = loadPath.unityPath;
      var request = Resources.LoadAsync<A>(path);
      return F.t(request, Future<Either<string, A>>.async(
        p => ASync.StartCoroutine(waitForLoadCoroutine<A>(request, p.complete, path))
      ));
    }

    [PublicAPI]
    public static Tpl<ResourceRequest, Future<A>> loadAsyncIgnoreErrors<A>(
      PathStr loadPath, bool logOnError = true
    ) where A : Object =>
      loadAsync<A>(loadPath).map2(future => future.dropError(logOnError));

    static IEnumerator waitForLoadCoroutine<A>(
      ResourceRequest request, Action<Either<string, A>> whenDone, string path
    ) where A : Object {
      yield return request;
      if (request.asset is A a) {
        whenDone(a);
      }
      else {
        whenDone(notFound<A>(path));
      }
    }
  }
}