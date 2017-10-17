using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using Smooth.Dispose;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Net {
  public class ImageDownloader {
    public static ImageDownloader instance = new ImageDownloader();

    readonly Dictionary<Url, Future<Either<WWWError, UsageCountedDisposable<Texture2D>>>> cache =
      new Dictionary<Url, Future<Either<WWWError, UsageCountedDisposable<Texture2D>>>>();

    readonly ASyncNAtATimeQueue<Url, Either<WWWError, UsageCountedDisposable<Texture2D>>> queue;

    ImageDownloader() {
      queue = new ASyncNAtATimeQueue<
        Url,
        Either<WWWError, UsageCountedDisposable<Texture2D>>
      >(2, download);
    }

    Future<Either<WWWError, UsageCountedDisposable<Texture2D>>> download(Url url) =>
      Future<Either<WWWError, UsageCountedDisposable<Texture2D>>>.async((promise, f) => {
        ASync.StartCoroutine(textureLoader(
          new WWW(url), promise,
          onDispose: t => {
            Object.Destroy(t);
            cache.Remove(url);
            if (Log.isDebug) Log.rdebug($"{nameof(ImageDownloader)} disposed texture: {url}");
          })
        );

        f.onComplete(e => {
          // remove from cache if image was not downloaded
          if (e.isLeft) cache.Remove(url);
        });
      });

    // TODO: make it possible to dispose image before it started downloading / while downloading
    public Future<Either<WWWError, Disposable<Texture2D>>> loadImage(Url url, bool ignoreQueue = false) =>
      cache
        .getOrUpdate(url, () => ignoreQueue ? download(url) : queue.enqueue(url))
        .mapT(dt => dt.use());

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