using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger {
  public static class ErrorReporter {
    public struct ErrorData {
      public readonly LogType errorType;
      public readonly string message;
      public readonly ImmutableList<BacktraceElem> backtrace;

      public ErrorData(LogType errorType, string message, ImmutableList<BacktraceElem> backtrace) {
        this.errorType = errorType;
        this.message = message;
        this.backtrace = backtrace;
      }

      public override string ToString() => 
        $"{nameof(ErrorData)}[" +
        $"{nameof(errorType)}: '{errorType}', " +
        $"{nameof(message)}: '{message}', " +
        $"{nameof(backtrace)}: {backtrace.asString()}" +
        $"]";
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

    public static void registerToUnity(OnError onError, bool logWarnings) {
      Application.logMessageReceivedThreaded += (message, backtrace, type) => {
        if (
          type == LogType.Assert || type == LogType.Error || type == LogType.Exception
          || (logWarnings && type == LogType.Warning)
        ) ASync.OnMainThread(
          () => {
            try { 
              var parsedBacktrace =
                // backtrace may be empty in release mode.
                string.IsNullOrEmpty(backtrace)
                ? BacktraceElem.generateFromHere()
                : BacktraceElem.parseUnityBacktrace(backtrace);
              var data = new ErrorData(type, message, parsedBacktrace);
              onError(data);
            }
            catch (Exception e) {
              // Log at info level so that we wouldn't trigger this handler again.
              Log.info(
                $"[{nameof(ErrorReporter)}] Exception in " +
                $"{nameof(Application)}.{nameof(Application.logMessageReceivedThreaded)}" +
                $" handler!\n\n{e}"
              );
            }
          },
          // https://fogbugz.unity3d.com/default.asp?832198_48nbh0a3a8cjpr12
          runNowIfOnMainThread: false
        );
      };
    }

    public static void trackWWWSend(string prefix, WWW www, Dictionary<string, string> headers) {
      ASync.StartCoroutine(ASync.WWWEnumerator(www).afterThis(() => {
        if (!string.IsNullOrEmpty(www.error)) {
          if (Log.isInfo) Log.info(
            prefix + " send failed with: " + www.error + 
            "\nRequest headers=" + headers.asString() +
            "\nResponse headers=" + www.responseHeaders.asString()
          );
        }
        else {
          if (Debug.isDebugBuild && Log.isInfo) Log.info(
            prefix + " send succeeded with response headers=" + www.responseHeaders.asString()
          );
        }
      }));
    }
  }
}
