using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.SceneManagement;
using static com.tinylabproductions.TLPLib.Data.typeclasses.Str;

namespace com.tinylabproductions.TLPLib.Logger.Reporting {
  public static class SentryAPI {
    public struct Tag : IEquatable<Tag> {
      // max tag value length = 200
      public readonly string s;

      public Tag(string s) { this.s = s.trimTo(200); }
      public static Tag a(object o) => new Tag(o.ToString());

      public override string ToString() => $"{nameof(SentryAPI)}.{nameof(Tag)}({s})";

      #region Equality

      public bool Equals(Tag other) => string.Equals(s, other.s);

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Tag && Equals((Tag) obj);
      }

      public override int GetHashCode() => (s != null ? s.GetHashCode() : 0);

      public static bool operator ==(Tag left, Tag right) { return left.Equals(right); }
      public static bool operator !=(Tag left, Tag right) { return !left.Equals(right); }

      #endregion

      public static implicit operator Tag(string s) => new Tag(s);
    }

    public enum LogLevel : byte { FATAL, ERROR, WARNING, INFO, DEBUG }
    public static class LogLevel_ {
      public static readonly Str<LogLevel> str = new LambdaStr<LogLevel>(s => {
        switch (s) {
          case LogLevel.FATAL: return "fatal";
          case LogLevel.ERROR: return "error";
          case LogLevel.WARNING: return "warning";
          case LogLevel.INFO: return "info";
          case LogLevel.DEBUG: return "debug";
          default:
            throw new ArgumentOutOfRangeException(nameof(s), s, null);
        }
      });
    }

    public struct DSN {
      public readonly Uri reportingUrl;
      public readonly string projectId;
      public readonly ApiKeys keys;

      public DSN(Uri reportingUrl, string projectId, ApiKeys keys) {
        this.reportingUrl = reportingUrl;
        this.projectId = projectId;
        this.keys = keys;
      }

      public override string ToString() =>
        $"{nameof(SentryAPI)}.{nameof(DSN)}[" +
        $"{nameof(reportingUrl)}: {reportingUrl}, " +
        $"{nameof(projectId)}: {projectId}, " +
        $"{nameof(keys)}: {keys}" +
        $"]";

      // Parses DSN like "http://public:secret@errors.tinylabproductions.com/project_id"
      public static Either<string, DSN> fromDSN(string dsn) {
        try {
          if (dsn == null)
            return "dsn is null!";
          dsn = dsn.Trim();
          if (dsn == "")
            return "dsn is empty!";
          var baseUri = new Uri(dsn);
          if (string.IsNullOrEmpty(baseUri.UserInfo))
            return $"user info in dsn '{dsn}' is empty!";
          var userInfoSplit = baseUri.UserInfo.Split(new[] {':'}, 2);
          if (userInfoSplit.Length != 2)
            return $"user info in dsn '{dsn}' is does not have secret key!";
          var keys = new ApiKeys(userInfoSplit[0], userInfoSplit[1]);
          var projectId = baseUri.LocalPath.Substring(1);
          var reportingUri = new Uri(
            $"{baseUri.Scheme}://{baseUri.Host}:{baseUri.Port}/api/{projectId}/store/"
          );
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

      public override string ToString() =>
        $"{nameof(SentryAPI)}.{nameof(ApiKeys)}[" +
        $"{nameof(publicKey)}={publicKey}, " +
        $"{nameof(secretKey)}={secretKey}" +
        $"]";
    }

    public struct ExtraData {
      /// <summary>
      /// This is needed, because tag values can change over the runtime. Therefore
      /// we want to add the tags to the data immediatelly before sending the data.
      /// </summary>
      public delegate void AddTag(string key, Tag tag);
      public delegate void AddExtra(string key, string extra);

      public readonly Act<AddTag> addTags;
      public readonly Act<AddExtra> addExtras;

      public ExtraData(Act<AddTag> addTags, Act<AddExtra> addExtras) {
        this.addTags = addTags;
        this.addExtras = addExtras;
      }

      public void addTagsToDictionary(IDictionary<string, Tag> dict) => addTags(dict.Add);
      public void addExtrasToDictionary(IDictionary<string, string> dict) => addExtras(dict.Add);
    }

    // https://docs.sentry.io/clientdev/interfaces/user/
    public struct UserInterface {
      public struct Id {
        public readonly string value;
        public Id(string value) { this.value = value; }
        public override string ToString() => $"{nameof(Id)}({value})";
      }

      public struct IpAddress {
        public readonly string value;
        public IpAddress(string value) { this.value = value; }
        public override string ToString() => $"{nameof(IpAddress)}({value})";
      }

      public readonly These<Id, IpAddress> uniqueIdentifier;
      public readonly Option<string> email, username;
      public readonly IDictionary<string, string> extras;

      public Option<Id> id => uniqueIdentifier.thisValue;
      public Option<IpAddress> ipAddress => uniqueIdentifier.thatValue;

      public UserInterface(
        These<Id, IpAddress> uniqueIdentifier,
        Option<string> email = default(Option<string>),
        Option<string> username = default(Option<string>),
        IDictionary<string, string> extras = null
      ) {
        Option.ensureValue(ref email);
        Option.ensureValue(ref username);

        this.uniqueIdentifier = uniqueIdentifier;
        this.email = email;
        this.username = username;
        this.extras = extras ?? new Dictionary<string, string>();
      }
    }

    public struct SendOnErrorData {
      public readonly ErrorReporter.AppInfo appInfo;
      public readonly ExtraData addExtraData;
      public readonly Option<UserInterface> userOpt;
      public readonly IDictionary<string, Tag> staticTags;
      public readonly IDictionary<string, string> staticExtras;

      public SendOnErrorData(
        ErrorReporter.AppInfo appInfo, ExtraData addExtraData,
        Option<UserInterface> userOpt,
        IDictionary<string, Tag> staticTags, IDictionary<string, string> staticExtras
      ) {
        this.appInfo = appInfo;
        this.addExtraData = addExtraData;
        this.userOpt = userOpt;
        this.staticTags = staticTags;
        this.staticExtras = staticExtras;
      }
    }

    /// <summary>Tags that might change during runtime.</summary>
    public static Dictionary<string, Tag> dynamicTags() => new Dictionary<string, Tag> {
      {"App:LoadedLevelNames", new Tag(
        Enumerable2.fromImperative(SceneManager.sceneCount, SceneManager.GetSceneAt).
          Select(_ => $"{_.name}({_.buildIndex})").OrderBy(_ => _).mkString(", ")
      )},
      {"App:InternetReachability", Tag.a(Application.internetReachability)},
      {"App:TargetFrameRate", Tag.a(Application.targetFrameRate)},
    };

    /// <summary>Tags that will never change during runtime.</summary>
    public static Dictionary<string, Tag> staticTags(ErrorReporter.AppInfo appInfo) =>
      new Dictionary<string, Tag> {
        // max tag name length = 32
        {"App:LevelCount", new Tag(s(SceneManager.sceneCountInBuildSettings))},
        {"App:UnityVersion", new Tag(Application.unityVersion)},
        {"App:BundleIdentifier", new Tag(Application.bundleIdentifier)},
        {"App:InstallMode", new Tag(Application.installMode.ToString())},
        {"App:SandboxType", new Tag(Application.sandboxType.ToString())},
        {"App:ProductName", new Tag(Application.productName)},
        {"App:CompanyName", new Tag(Application.companyName)},
        {"App:CloudProjectId", new Tag(Application.cloudProjectId)},
        {"App:SystemLanguage", new Tag(Application.systemLanguage.ToString())},
        {"App:BackgroundLoadingPriority", new Tag(Application.backgroundLoadingPriority.ToString())},
        {"App:GenuineCheckAvailable", new Tag(s(Application.genuineCheckAvailable))},
        {"App:Genuine", new Tag(s(Application.genuineCheckAvailable && Application.genuine))},
        {"SI:ProcessorCount", new Tag(s(SystemInfo.processorCount))},
        {"SI:GraphicsMemorySize", new Tag(s(SystemInfo.graphicsMemorySize))},
        {"SI:GraphicsDeviceName", new Tag(SystemInfo.graphicsDeviceName)},
        {"SI:GraphicsDeviceVendor", new Tag(SystemInfo.graphicsDeviceVendor)},
        {"SI:GraphicsDeviceID", new Tag(s(SystemInfo.graphicsDeviceID))},
        {"SI:GraphicsDeviceVendorID", new Tag(s(SystemInfo.graphicsDeviceVendorID))},
        {"SI:GraphicsDeviceType", new Tag(SystemInfo.graphicsDeviceType.ToString())},
        {"SI:GraphicsDeviceVersion", new Tag(SystemInfo.graphicsDeviceVersion)},
        {"SI:GraphicsShaderLevel", new Tag(s(SystemInfo.graphicsShaderLevel))},
        {"SI:GraphicsMultiThreaded", new Tag(s(SystemInfo.graphicsMultiThreaded))},
        {"SI:SupportsShadows", new Tag(s(SystemInfo.supportsShadows))},
        {"SI:SupportsRenderToCubemap", new Tag(s(SystemInfo.supportsRenderToCubemap))},
        {"SI:SupportsImageEffects", new Tag(s(SystemInfo.supportsImageEffects))},
        {"SI:Supports3DTextures", new Tag(s(SystemInfo.supports3DTextures))},
        {"SI:SupportsComputeShaders", new Tag(s(SystemInfo.supportsComputeShaders))},
        {"SI:SupportsInstancing", new Tag(s(SystemInfo.supportsInstancing))},
        {"SI:SupportsSparseTextures", new Tag(s(SystemInfo.supportsSparseTextures))},
        {"SI:SupportedRenderTargetCount", new Tag(s(SystemInfo.supportedRenderTargetCount))},
        {"SI:NPOTsupport", new Tag(SystemInfo.npotSupport.ToString())},
        {"SI:DeviceName", new Tag(SystemInfo.deviceName)},
        {"SI:SupportsAccelerometer", new Tag(s(SystemInfo.supportsAccelerometer))},
        {"SI:SupportsGyroscope", new Tag(s(SystemInfo.supportsGyroscope))},
        {"SI:SupportsLocationService", new Tag(s(SystemInfo.supportsLocationService))},
        {"SI:SupportsVibration", new Tag(s(SystemInfo.supportsVibration))},
        {"SI:DeviceType", new Tag(SystemInfo.deviceType.ToString())},
        {"SI:MaxTextureSize", new Tag(s(SystemInfo.maxTextureSize))},
        {"ProductName", new Tag(appInfo.productName)},
        {"BundleIdentifier", new Tag(appInfo.bundleIdentifier)},
        {"App:Platform", new Tag(Application.platform.ToString())},
        {"SI:OperatingSystem", new Tag(SystemInfo.operatingSystem)},
        {"SI:ProcessorType", new Tag(SystemInfo.processorType)},
        {"SI:SystemMemorySize", new Tag(s(SystemInfo.systemMemorySize))},
        {"SI:DeviceModel", new Tag(SystemInfo.deviceModel)}
#if !UNITY_5_5_OR_NEWER
        {"SI:SupportsRenderTextures", new Tag(s(SystemInfo.supportsRenderTextures))},
        {"SI:SupportsStencil", new Tag(s(SystemInfo.supportsStencil))},
        {"App:WebSecurityEnabled", new Tag(s(Application.webSecurityEnabled))},
        {"App:WebSecurityHostUrl", new Tag(Application.webSecurityHostUrl)},
#endif
      };

    public static Dictionary<string, string> dynamicExtras() => new Dictionary<string, string> {
      {"App:StreamedBytes", s(Application.streamedBytes)},
    };

    public static Dictionary<string, string> staticExtras = new Dictionary<string, string>();

    public static LogLevel asSentry(this Log.Level type) {
      switch (type) {
        case Log.Level.ERROR:
          return LogLevel.ERROR;
        case Log.Level.WARN:
          return LogLevel.WARNING;
        case Log.Level.INFO:
          return LogLevel.INFO;
        case Log.Level.DEBUG:
        case Log.Level.VERBOSE:
          return LogLevel.INFO;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }
    }

    public static ErrorReporter.OnError createLogOnError(
      SendOnErrorData sendData, DSN dsn
    ) => logEvent => {
      if (Log.d.isInfo()) Log.d.info(
        $"Sentry error:\n\n{logEvent}\nreporting url={dsn.reportingUrl}\n" +
        SentryRESTAPI.message(
          dsn.keys, sendData.appInfo, logEvent, sendData.addExtraData,
          sendData.userOpt, sendData.staticTags
        )
      );
    };

    public static IEnumerable<KeyValuePair<string, Tag>> convertTags(
      ImmutableArray<Tpl<string, string>> source
    ) => source.Select(t => F.kv(t._1, new Tag(t._2)));

    public static IEnumerable<KeyValuePair<string, string>> convertExtras(
      ImmutableArray<Tpl<string, string>> source
    ) => source.Select(t => F.kv(t._1, t._2));
  }

  public static class SentryRESTAPI {
    public struct Message {
      public readonly SentryAPI.ApiKeys keys;
      public readonly Guid eventId;
      public readonly DateTime timestamp;
      public readonly string json;

      readonly Dictionary<string, object> jsonDict;

      public Message(
        SentryAPI.ApiKeys keys, Guid eventId, DateTime timestamp,
        Dictionary<string, object> jsonDict
      ) {
        this.keys = keys;
        this.eventId = eventId;
        this.timestamp = timestamp;
        this.jsonDict = jsonDict;

        var json = new Dictionary<string, object>(jsonDict) {
          {"event_id", eventId.ToString("N")},
          {"timestamp", timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss")}
        };
        this.json = Json.Serialize(json);
      }

      public string jsonWithoutTimestamps => Json.Serialize(jsonDict);

      public override string ToString() { return $"SentryMessage[\n{keys}\ntimestamp={timestamp}\njson={json}\n]"; }

      public WWW send(Uri reportingUrl) {
        const string userAgent = "raven-unity-tlplib/HEAD";
        var headers = new Dictionary<string, string> {
          {"User-Agent", userAgent},
          {"Content-Type", "application/json"},
          {
            "X-Sentry-Auth",
            $"Sentry sentry_version=7, sentry_client={userAgent}, " +
            $"sentry_timestamp={timestamp.toUnixTimestamp()}, " +
            $"sentry_key={keys.publicKey}, sentry_secret={keys.secretKey}"
          }
        };
        var www = new WWW(reportingUrl.OriginalString, Encoding.UTF8.GetBytes(json), headers);
        www.trackWWWSend("Sentry API", headers);
        return www;
      }
    }

    // ReSharper disable once UnusedMember.Global
    public static ErrorReporter.OnError createSendOnError(
      SentryAPI.SendOnErrorData sendData, Uri reportingUrl, SentryAPI.ApiKeys keys, bool onlySendUniqueErrors
    ) {
      var sentJsonsOpt = onlySendUniqueErrors.opt(() => new HashSet<string>());

      return logEvent => {
        var msg = message(
          keys, sendData.appInfo, logEvent, sendData.addExtraData,
          sendData.userOpt, sendData.staticTags
        );
        Action send = () => msg.send(reportingUrl);
        sentJsonsOpt.voidFold(
          send,
          sentJsons => {
            if (sentJsons.Add(msg.jsonWithoutTimestamps)) send();
            else if (Log.d.isDebug()) Log.d.debug($"Not sending duplicate Sentry msg: {msg}");
          }
        );
      };
    }

    public static Message message(
      SentryAPI.ApiKeys keys, ErrorReporter.AppInfo appInfo,
      LogEvent data, SentryAPI.ExtraData extraData, Option<SentryAPI.UserInterface> userOpt,
      IDictionary<string, SentryAPI.Tag> staticTags
    ) {
      const string logger = "tlplib-" + nameof(SentryRESTAPI);
      var eventId = Guid.NewGuid();
      var timestamp = DateTime.UtcNow;

      // The list of frames should be ordered by the oldest call first.
      var stacktraceFrames = data.entry.backtrace.map(
        // ReSharper disable once ConvertClosureToMethodGroup - MCS bug
        b => b.elements.a.Select(a => backtraceElemToJson(a)).Reverse().ToList()
      );

      // Beware of the order! Do not mutate static tags!
      var tags =
        SentryAPI.dynamicTags()
        .addAll(staticTags)
        .addAll(SentryAPI.convertTags(data.entry.tags));
      extraData.addTagsToDictionary(tags);

      // Extra contextual data is limited to 4096 characters.
      var extras =
        SentryAPI.dynamicExtras()
        .addAll(SentryAPI.convertExtras(data.entry.extras));
      extraData.addExtrasToDictionary(extras);

      var json = new Dictionary<string, object> {
        {"message", s(data.entry.message)},
        {"level", s(data.level.asSentry(), SentryAPI.LogLevel_.str)},
        {"logger", s(logger)},
        {"platform", s(Application.platform.ToString())},
        {"release", s(appInfo.bundleVersion)},
        {"tags", tags.toDict(_ => _.Key, _ => _.Value.s)},
        {"extra", extras},
        {"fingerprint", data.entry.toSentryFingerprint()}
      };
      foreach (var stacktrace in stacktraceFrames)
        json.Add("stacktrace", new Dictionary<string, object> {{"frames", stacktrace}});
      foreach (var user in userOpt)
        json.Add("user", userInterfaceParametersJson(user));

      return new Message(keys, eventId, timestamp, json);
    }

    static Dictionary<string, object> userInterfaceParametersJson(SentryAPI.UserInterface user) {
      var userDict = new Dictionary<string, object>();
      foreach (var value in user.id) userDict.Add("id", value.value);
      foreach (var value in user.username) userDict.Add("username", value);
      foreach (var value in user.email) userDict.Add("email", value);
      foreach (var value in user.ipAddress) userDict.Add("ip_address", value.value);
      foreach (var kv in user.extras) userDict.Add(kv.Key, kv.Value);
      return userDict;
    }

    static Dictionary<string, object> backtraceElemToJson(this BacktraceElem bt) {
      var json = new Dictionary<string, object> {
        {"function", bt.method},
        {"in_app", bt.inApp}
      };
      if (bt.fileInfo.isSome) {
        var fi = bt.fileInfo.get;
        json.Add("lineno", fi.lineNo);
        json.Add("filename", fi.file);
      }
      return json;
    }
  }

  public static class LogEventExts {
    /// <summary>
    /// An array of strings used to dictate the deduplication of this event.
    /// A value of {{ default }} will be replaced with the built-in behavior,
    /// thus allowing you to extend it, or completely replace it.
    ///
    /// https://docs.sentry.io/clientdev/attributes/
    /// </summary>
    public static string[] toSentryFingerprint(this LogEntry e) =>
      new[] {"{{ default }}", e.message};
  }
}
