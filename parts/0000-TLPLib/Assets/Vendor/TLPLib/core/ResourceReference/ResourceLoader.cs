using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public static class ResourceLoader {
    static ErrorMsg notFound<A>(string path) => new ErrorMsg(
      $"Resource of type {typeof(A).FullName} not found at: {path}"
    );
    
    [PublicAPI]
    public static Either<ErrorMsg, A> load<A>(PathStr loadPath) where A : Object {
      var path = loadPath.unityPath;
      var a = Resources.Load<A>(path);
      if (a) return a;
      return notFound<A>(path);
    }

    public static Tpl<IAsyncOperation, Future<Either<ErrorMsg, A>>> loadAsync<A>(
      PathStr loadPath
    ) where A : Object {
      var path = loadPath.unityPath;
      IResourceRequest request = new WrappedResourceRequest(Resources.LoadAsync<A>(path));
      return F.t(
        request.upcast(default(IAsyncOperation)), 
        Future.async<Either<ErrorMsg, A>>(
          p => ASync.StartCoroutine(waitForLoadCoroutine<A>(request, p.complete, path))
        )
      );
    }

    [PublicAPI]
    public static Tpl<IAsyncOperation, Future<A>> loadAsyncIgnoreErrors<A>(
      PathStr loadPath, bool logOnError = true
    ) where A : Object =>
      loadAsync<A>(loadPath).map2(future => future.dropError(logOnError));

    static IEnumerator waitForLoadCoroutine<A>(
      IResourceRequest request, Action<Either<ErrorMsg, A>> whenDone, string path
    ) where A : Object {
      yield return request.yieldInstruction;
      if (request.asset is A a) {
        whenDone(a);
      }
      else {
        whenDone(notFound<A>(path));
      }
    }
  }
}