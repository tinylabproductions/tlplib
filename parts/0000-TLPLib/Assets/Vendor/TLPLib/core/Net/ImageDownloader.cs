using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using pzd.lib.exts;
using pzd.lib.functional;
using Smooth.Dispose;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Net {
  public partial class ImageDownloader {
    [Record]
    partial struct InternalResult {
      public readonly Future<Either<WWWError, UsageCountedDisposable<Texture2D>>> future;
      
      public Result toResult() => new Result(future.mapT(_ => _.use()));
    } 
    
    [Record]
    public partial struct Result {
      public readonly Future<Either<WWWError, Disposable<Texture2D>>> future;
    }
    
    public static readonly ImageDownloader instance = new ImageDownloader();

    readonly Dictionary<Url, InternalResult> cache = new Dictionary<Url, InternalResult>();
    readonly ASyncNAtATimeQueue<Url, Either<WWWError, UsageCountedDisposable<Texture2D>>> queue;

    ImageDownloader() {
      queue = new ASyncNAtATimeQueue<
        Url,
        Either<WWWError, UsageCountedDisposable<Texture2D>>
      >(2, url => download(url).future);
    }

    InternalResult download(Url url) => new InternalResult(
      Future<Either<WWWError, UsageCountedDisposable<Texture2D>>>.async((promise, f) => {
        ASync.StartCoroutine(textureLoader(
          // TODO: change to UnityWebRequest.GetTexture from old WWW implementation
          // Here is sample code, but I don't remember why it is not used. Try it and see
          // for yourself.
//          var f = UnityWebRequest.GetTexture(staticAd.image.url).toFuture().flatMapT(req => {
//            var dlHandler = (DownloadHandlerTexture) req.downloadHandler;
//            var texture = dlHandler.texture;
//            if (texture) return new Either<ErrorMsg, Texture>(texture);
//            return new ErrorMsg($"Can't download texture from url{staticAd.image.url}");
//          });
          new WWW(url), promise,
          onDispose: t => {
            Object.Destroy(t);
            cache.Remove(url);
            if (Log.d.isDebug()) Log.d.debug($"{nameof(ImageDownloader)} disposed texture: {url}");
          })
        );

        f.onComplete(e => {
          // remove from cache if image was not downloaded
          if (e.isLeft) cache.Remove(url);
        });
      })
    );

    // TODO: make it possible to dispose image before it started downloading / while downloading
    public Result loadImage(Url url, bool ignoreQueue = false) =>
      cache
        .getOrUpdate(url, () => ignoreQueue ? download(url) : new InternalResult(queue.enqueue(url)))
        .toResult();

    static IEnumerator textureLoader(
      WWW www,
      Promise<Either<WWWError, UsageCountedDisposable<Texture2D>>> promise,
      Action<Texture2D> onDispose
    ) {
      yield return www;
      promise.complete(
        string.IsNullOrEmpty(www.error)
          ? WWWExts.asTexture(www).mapRight(t => UsageCountedDisposable.a(t, onDispose))
          : new WWWError(www, www.error)
      );
    }
  }
}