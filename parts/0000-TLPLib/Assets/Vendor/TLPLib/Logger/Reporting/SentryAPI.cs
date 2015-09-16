using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger.Reporting {
  public static class SentryAPI {
    public struct DSN {
      public readonly Uri reportingUrl;
      public readonly string projectId;
      public readonly ApiKeys keys;

      public DSN(Uri reportingUrl, string projectId, ApiKeys keys) {
        this.reportingUrl = reportingUrl;
        this.projectId = projectId;
        this.keys = keys;
      }

      // Parses DSN like "http://public:secret@errors.tinylabproductions.com/project_id"
      public static Either<string, DSN> fromDSN(string dsn) {
        try {
          var baseUri = new Uri(dsn);
          if (string.IsNullOrEmpty(baseUri.UserInfo))
            return ("user info in dsn '" + dsn + "' is empty!").left().r<DSN>();
          var userInfoSplit = baseUri.UserInfo.Split(new[] {':'}, 2);
          if (userInfoSplit.Length != 2)
            return ("user info in dsn '" + dsn + "' is does not have secret key!").left().r<DSN>();
          var keys = new ApiKeys(userInfoSplit[0], userInfoSplit[1]);
          var projectId = baseUri.LocalPath.Substring(1);
          var reportingUri = new Uri($"{baseUri.Scheme}://{baseUri.Host}/api/{projectId}/store/");
          return new DSN(reportingUri, projectId, keys).right().l<string>();
        }
        catch (Exception e) {
          return e.ToString().left().r<DSN>();
        }
      }
    }

    public struct ApiKeys {
      public readonly string publicKey, secretKey;

      public ApiKeys(string publicKey, string secretKey) {
        this.publicKey = publicKey;
        this.secretKey = secretKey;
      }

      public override string ToString() { return $"SentryAPI.ApiKeys[public={publicKey}, secret={secretKey}]"; }
    }

    public struct SentryMessage {
      public readonly ApiKeys keys;
      public readonly DateTime timestamp;
      public readonly string json;

      public SentryMessage(ApiKeys keys, DateTime timestamp, string json) {
        this.keys = keys;
        this.timestamp = timestamp;
        this.json = json;
      }

      public override string ToString() { return $"SentryMessage[\n{keys}\ntimestamp={timestamp}\njson={json}\n]"; }

      public WWW send(Uri reportingUrl) {
        const string userAgent = "raven-unity-tlplib/HEAD";
        var headers = new Dictionary<string, string> {
          {"User-Agent", userAgent},
          {"Content-Type", "application/json"}, {
            "X-Sentry-Auth",
            $"Sentry sentry_version=7, sentry_client={userAgent}, " +
            $"sentry_timestamp={timestamp.toUnixTimestamp()}, " +
            $"sentry_key={keys.publicKey}, sentry_secret={keys.secretKey}"
          }
        };
        var www = new WWW(reportingUrl.OriginalString, Encoding.UTF8.GetBytes(json), headers);
        ErrorReporter.trackWWWSend("Sentry API", www, headers);
        return www;
      }
    }

    public struct ExtraData {
      public delegate void AddTag(string name, string value);
      public delegate void AddExtra(string name, string value);

      public readonly static ExtraData noExtraData = new ExtraData(_ => {}, _ => {});

      public readonly Act<AddTag> addTags;
      public readonly Act<AddExtra> addExtras;

      public ExtraData(
        Act<AddTag> addTags = null, 
        Act<AddExtra> addExtras = null
      ) {
        this.addTags = addTags ?? (_ => {});
        this.addExtras = addExtras ?? (_ => {});
      }
    }

    public static ErrorReporter.OnError createOnError(
      string loggerName, Uri reportingUrl, ApiKeys keys, ErrorReporter.AppInfo appInfo,
      ExtraData addExtraData
    ) {
      return (data => {
        var msg = message(loggerName, keys, appInfo, data, addExtraData);
        ASync.NextFrame(() => msg.send(reportingUrl));
      });
    }

    public static ErrorReporter.OnError createEditorOnError(
      string loggerName, DSN dsn, ErrorReporter.AppInfo appInfo,
      ExtraData addExtraData
    ) {
      return (data => ASync.NextFrame(() => {
        if (Log.isInfo) Log.info(
          $"Sentry error:\n\n{data}\nreporting url={dsn.reportingUrl}\n" +
          message(loggerName, dsn.keys, appInfo, data, addExtraData)
        );
      }));
    }

    public static SentryMessage message(
      string loggerName, ApiKeys keys, ErrorReporter.AppInfo appInfo,
      ErrorReporter.ErrorData data, ExtraData addExtraData
    ) {
      var timestamp = DateTime.UtcNow;

      // The list of frames should be ordered by the oldest call first.
      // ReSharper disable once ConvertClosureToMethodGroup - MCS bug
      var stacktraceFrames = data.backtrace.Select(a => backtraceElemToJson(a)).Reverse().ToList();

      // Tags are properties that can be filtered/grouped by.
      var tags = new Dictionary<string, object> {
        // max tag name length = 32
        {"ProductName", tag(appInfo.productName)},
        {"BundleIdentifier", tag(appInfo.bundleIdentifier)},
        {"App:LoadedLevel", tag(Application.loadedLevel)},
        {"App:LoadedLevelName", tag(Application.loadedLevelName)},
        {"App:IsLoadingLevel", tag(Application.isLoadingLevel)},
        {"App:LevelCount", tag(Application.levelCount)},
        {"App:Platform", tag(Application.platform)},
        {"App:UnityVersion", tag(Application.unityVersion)},
        {"App:Version", tag(Application.version)},
        {"App:BundleIdentifier", tag(Application.bundleIdentifier)},
        {"App:InstallMode", tag(Application.installMode)},
        {"App:SandboxType", tag(Application.sandboxType)},
        {"App:ProductName", tag(Application.productName)},
        {"App:CompanyName", tag(Application.companyName)},
        {"App:CloudProjectId", tag(Application.cloudProjectId)},
        {"App:WebSecurityEnabled", tag(Application.webSecurityEnabled)},
        {"App:WebSecurityHostUrl", tag(Application.webSecurityHostUrl)},
        {"App:TargetFrameRate", tag(Application.targetFrameRate)},
        {"App:SystemLanguage", tag(Application.systemLanguage)},
        {"App:BackgroundLoadingPriority", tag(Application.backgroundLoadingPriority)},
        {"App:InternetReachability", tag(Application.internetReachability)},
        {"App:GenuineCheckAvailable", tag(Application.genuineCheckAvailable)},
        {"App:Genuine", tag(Application.genuineCheckAvailable && Application.genuine)},
        {"SI:OperatingSystem", tag(SystemInfo.operatingSystem)},
        {"SI:ProcessorType", tag(SystemInfo.processorType)},
        {"SI:ProcessorCount", tag(SystemInfo.processorCount)},
        {"SI:SystemMemorySize", tag(SystemInfo.systemMemorySize)},
        {"SI:GraphicsMemorySize", tag(SystemInfo.graphicsMemorySize)},
        {"SI:GraphicsDeviceName", tag(SystemInfo.graphicsDeviceName)},
        {"SI:GraphicsDeviceVendor", tag(SystemInfo.graphicsDeviceVendor)},
        {"SI:GraphicsDeviceID", tag(SystemInfo.graphicsDeviceID)},
        {"SI:GraphicsDeviceVendorID", tag(SystemInfo.graphicsDeviceVendorID)},
        {"SI:GraphicsDeviceType", tag(SystemInfo.graphicsDeviceType)},
        {"SI:GraphicsDeviceVersion", tag(SystemInfo.graphicsDeviceVersion)},
        {"SI:GraphicsShaderLevel", tag(SystemInfo.graphicsShaderLevel)},
        {"SI:GraphicsMultiThreaded", tag(SystemInfo.graphicsMultiThreaded)},
        {"SI:SupportsShadows", tag(SystemInfo.supportsShadows)},
        {"SI:SupportsRenderTextures", tag(SystemInfo.supportsRenderTextures)},
        {"SI:SupportsRenderToCubemap", tag(SystemInfo.supportsRenderToCubemap)},
        {"SI:SupportsImageEffects", tag(SystemInfo.supportsImageEffects)},
        {"SI:Supports3DTextures", tag(SystemInfo.supports3DTextures)},
        {"SI:SupportsComputeShaders", tag(SystemInfo.supportsComputeShaders)},
        {"SI:SupportsInstancing", tag(SystemInfo.supportsInstancing)},
        {"SI:SupportsSparseTextures", tag(SystemInfo.supportsSparseTextures)},
        {"SI:SupportedRenderTargetCount", tag(SystemInfo.supportedRenderTargetCount)},
        {"SI:SupportsStencil", tag(SystemInfo.supportsStencil)},
        {"SI:NPOTsupport", tag(SystemInfo.npotSupport)},
        {"SI:DeviceName", tag(SystemInfo.deviceName)},
        {"SI:DeviceModel", tag(SystemInfo.deviceModel)},
        {"SI:SupportsAccelerometer", tag(SystemInfo.supportsAccelerometer)},
        {"SI:SupportsGyroscope", tag(SystemInfo.supportsGyroscope)},
        {"SI:SupportsLocationService", tag(SystemInfo.supportsLocationService)},
        {"SI:SupportsVibration", tag(SystemInfo.supportsVibration)},
        {"SI:DeviceType", tag(SystemInfo.deviceType)},
        {"SI:MaxTextureSize", tag(SystemInfo.maxTextureSize)},
      };
      addExtraData.addTags((name, value) => tags[name] = tag(value));

      // Extra contextual data is limited to 4096 characters.
      var extras = new Dictionary<string, object> {
        {"App:StreamedBytes", Application.streamedBytes},
      };
      addExtraData.addExtras((name, value) => extras[name] = value);

      var json = new Dictionary<string, object> {
        {"event_id", Guid.NewGuid().ToString("N")},
        // max length - 1000 chars
        {"message", data.message.trimTo(1000)},
        {"timestamp", timestamp.ToString("yyyy-MM-ddTHH:mm:ss")},
        {"level", logTypeToSentryLevel(data.errorType)},
        {"logger", loggerName},
        {"platform", Application.platform.ToString()},
        {"release", appInfo.bundleVersion},
        {"tags", tags},
        {"extra", extras},
        {"stacktrace", new Dictionary<string, object> {{"frames", stacktraceFrames}}}
      };
      if (!data.backtrace.isEmpty()) json.Add("culprit", data.backtrace[0].method);

      var serialized = Formats.MiniJSON.Json.Serialize(json);
      return new SentryMessage(keys, timestamp, serialized);
    }

    // max tag value length = 200
    static string tag(object o) { return o.ToString().trimTo(200); }

    public static string logTypeToSentryLevel(LogType type) {
      switch (type) {
        case LogType.Assert:
        case LogType.Error: 
        case LogType.Exception:
          return "error";
        case LogType.Log:
          return "debug";
        case LogType.Warning:
          return "warning";
      }
      throw new IllegalStateException("unreachable code");
    }

    public static Dictionary<string, object> backtraceElemToJson(this BacktraceElem bt) {
      var json = new Dictionary<string, object> {
        {"function", bt.method},
        {"in_app", bt.inApp}
      };
      if (bt.fileInfo.isDefined) {
        var fi = bt.fileInfo.get;
        json.Add("lineno", fi.lineNo);
        json.Add("filename", fi.file);
      }
      return json;
    }
  }
}
