using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using com.tinylabproductions.TLPLib.Concurrent.unity_web_request;
using com.tinylabproductions.TLPLib.Data;
using pzd.lib.log;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine.Networking;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityWebRequestExts {
    [PublicAPI]
    public static Future<Either<WebRequestError, byte[]>> downloadToRam(
      this UnityWebRequest req, AcceptedResponseCodes acceptedResponseCodes
    ) {
      var handler = 
        req.downloadHandler is DownloadHandlerBuffer h 
          ? h 
          : new DownloadHandlerBuffer();
      req.downloadHandler = handler;
      return req.toFuture(acceptedResponseCodes, _ => handler.data);
    }

    [PublicAPI]
    public static Future<Either<LogEntry, byte[]>> downloadToRamSimpleError(
      this UnityWebRequest req, AcceptedResponseCodes acceptedResponseCodes
    ) => req.downloadToRam(acceptedResponseCodes).map(_ => _.mapLeft(err => err.simplify));
  }
}