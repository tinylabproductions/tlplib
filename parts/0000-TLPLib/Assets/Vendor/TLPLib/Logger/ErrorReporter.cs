using System.Collections.Generic;
using System.Collections.ObjectModel;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger {
  public static class ErrorReporter {
    public struct ErrorData {
      public readonly LogType errorType;
      public readonly string message;
      public readonly ReadOnlyCollection<BacktraceElem> backtrace;

      public ErrorData(LogType errorType, string message, ReadOnlyCollection<BacktraceElem> backtrace) {
        this.errorType = errorType;
        this.message = message;
        this.backtrace = backtrace;
      }

      public override string ToString() { return string.Format(
        "ErrorData[\n  errorType: {0}\n  message: {1}\n  backtrace: {2}\n]",
        errorType, message, backtrace.asString()
      ); }
    }

    public struct AppInfo {
      public readonly string bundleIdentifier, bundleVersion, productName;

      public AppInfo(string bundleIdentifier, string bundleVersion, string productName) {
        this.bundleIdentifier = bundleIdentifier;
        this.bundleVersion = bundleVersion;
        this.productName = productName;
      }
    }

    public delegate void OnError(ErrorData data);

    static public void registerToUnity(OnError onError, bool logWarnings) {
      Application.logMessageReceivedThreaded += (message, backtrace, type) => {
        if (
          type == LogType.Assert || type == LogType.Error || type == LogType.Exception
          || (logWarnings && type == LogType.Warning)
        ) onError(new ErrorData(type, message, BacktraceElem.parseUnityBacktrace(backtrace).AsReadOnly()));
      };
    }

    static public void trackWWWSend(string prefix, WWW www, Dictionary<string, string> headers) {
      ASync.StartCoroutine(ASync.WWWEnumerator(www).afterThis(() => {
        if (!string.IsNullOrEmpty(www.error)) Log.debug(
          prefix + " send failed with: " + www.error + 
          "\nRequest headers=" + headers.asString() +
          "\nResponse headers=" + www.responseHeaders.asString()
        );
        else if (Debug.isDebugBuild) Log.debug(
          prefix + " send succeeded with response headers=" + www.responseHeaders.asString()
        );
      }));
    }
  }
}
