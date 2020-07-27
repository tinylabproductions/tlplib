using System.Collections;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;
// obsolete WWW
#pragma warning disable 618

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static partial class ASync {
    /* Do async cancellable WWW request. */
    public static Cancellable<Future<Either<Cancelled, Either<WWWError, WWW>>>> toFuture(this WWW www) {
      Promise<Either<Cancelled, Either<WWWError, WWW>>> promise;
      var f = Future.async<Either<Cancelled, Either<WWWError, WWW>>>(out promise);

      var wwwCoroutine = StartCoroutine(WWWEnumerator(www, promise));

      return Cancellable.a(f, () => {
        if (www.isDone) return false;

        wwwCoroutine.stop();
        www.Dispose();
        promise.complete(new Either<Cancelled, Either<WWWError, WWW>>(Cancelled.instance));
        return true;
      });
    }  
    
    [PublicAPI]
    public static Cancellable<Future<Either<Cancelled, Either<WWWError, Texture2D>>>> asTexture(
      this Cancellable<Future<Either<Cancelled, Either<WWWError, WWW>>>> cancellable
    ) => cancellable.map(f => f.map(e => e.mapRight(_ => _.asTexture())));

    public static IEnumerator WWWEnumerator(WWW www) { yield return www; }

    public static IEnumerator WWWEnumerator(WWW www, Promise<Either<Cancelled, Either<WWWError, WWW>>> promise) =>
      WWWEnumerator(www).afterThis(() => promise.complete(
        Either<Cancelled, Either<WWWError, WWW>>.Right(www.toEither())
      ));
  }
}