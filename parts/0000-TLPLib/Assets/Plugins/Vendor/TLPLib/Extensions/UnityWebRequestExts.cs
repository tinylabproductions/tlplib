using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityWebRequestExts {
    [PublicAPI]
    public static Future<Either<WebRequestError, byte[]>> downloadToRam(
      this UnityWebRequest req
    ) {
      var handler = 
        req.downloadHandler is DownloadHandlerBuffer h 
          ? h 
          : new DownloadHandlerBuffer();
      req.downloadHandler = handler;
      return req.toFuture(_ => handler.data);
    }
  }
}