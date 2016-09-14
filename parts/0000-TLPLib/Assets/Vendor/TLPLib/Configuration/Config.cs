using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Configuration {
  /* See IConfig. */
  public class Config : ConfigBase {
    #region Fetch errors

    public abstract class ConfigFetchError {
      public readonly Urls urls;
      public readonly string message;

      protected ConfigFetchError(Urls urls, string message) {
        this.message = message;
        this.urls = urls;
      }

      public override string ToString() { return $"{nameof(ConfigFetchError)}[{urls}, {message}]"; }
    }

    public class ConfigTimeoutError : ConfigFetchError {
      public readonly Duration timeout;

      public ConfigTimeoutError(Urls urls, Duration timeout)
      : base(urls, $"Timed out: {timeout}")
      { this.timeout = timeout; }
    }

    public class ConfigWWWError : ConfigFetchError {
      public readonly WWWError error;

      public ConfigWWWError(Urls urls, WWWError error)
      : base(urls, $"WWW error: {error.error}")
      { this.error = error; }
    }

    public class WrongContentType : ConfigFetchError {
      public readonly string expectedContentType, actualContentType;

      public WrongContentType(Urls urls, string expectedContentType, string actualContentType)
      : base(
        urls, $"Expected 'Content-Type' to be '{expectedContentType}', but it was '{actualContentType}'"
      ) {
        this.expectedContentType = expectedContentType;
        this.actualContentType = actualContentType;
      }
    }

    #endregion

    public struct ParsingError {
      public readonly string jsonString;

      public ParsingError(string jsonString) {
        this.jsonString = jsonString;
      }
    }

    public struct Urls : IEquatable<Urls> {
      // C# calls URLs URIs. See http://stackoverflow.com/a/1984225/935259 for distinction.
      /** Actual URL this config needs to be fetched. **/
      public readonly Uri fetchUrl;
      /**
       * URL used in reporting. For example you might want to not
       * include timestamp when sending the URL to your error logger.
       **/
      public readonly Uri reportUrl;

      public Urls(Uri fetchUrl) : this(fetchUrl, fetchUrl) {}

      public Urls(Uri fetchUrl, Uri reportUrl) {
        this.fetchUrl = fetchUrl;
        this.reportUrl = reportUrl;
      }

      public override string ToString() =>
        $"{nameof(Config)}.{nameof(Urls)}[{reportUrl}]";

      #region Equality

      public bool Equals(Urls other) {
        return Equals(fetchUrl, other.fetchUrl) && Equals(reportUrl, other.reportUrl);
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Urls && Equals((Urls) obj);
      }

      public override int GetHashCode() {
        unchecked { return ((fetchUrl != null ? fetchUrl.GetHashCode() : 0) * 397) ^ (reportUrl != null ? reportUrl.GetHashCode() : 0); }
      }

      public static bool operator ==(Urls left, Urls right) { return left.Equals(right); }
      public static bool operator !=(Urls left, Urls right) { return !left.Equals(right); }

      #endregion
    }

    /**
     * Fetches JSON config from URL. Checks its content type.
     *
     * Throws WrongContentType if unexpected content type is found.
     **/
    public static Future<Either<ConfigFetchError, string>> fetch(
      Urls urls, Duration timeout, string expectedContentType="application/json"
    ) {
      return new WWW(urls.fetchUrl.ToString()).wwwFuture()
        .timeout(timeout).map(wwwE =>
          wwwE.map(
            _ => (ConfigFetchError) new ConfigTimeoutError(urls, timeout),
            e => e.mapLeft(err => (ConfigFetchError) new ConfigWWWError(urls, err))
          )
          .flatten()
        )
        .map(wwwE => wwwE.fold(
          Either<ConfigFetchError, string>.Left,
          www => {
            var contentType = www.responseHeaders.get("CONTENT-TYPE").getOrElse("undefined");
            // Sometimes we get redirected to internet paygate, which returns HTML
            // instead of our content.
            if (contentType != expectedContentType)
              return Either<ConfigFetchError, string>.Left(
                new WrongContentType(urls, expectedContentType, contentType)
              );

            return Either<ConfigFetchError, string>.Right(www.text);
          })
        );
    }

    public static Either<ParsingError, IConfig> parseJson(string json) {
      var jsonDict = (Dictionary<string, object>)Json.Deserialize(json);
      return jsonDict == null
        ? Either<ParsingError, IConfig>.Left(new ParsingError(json))
        : Either<ParsingError, IConfig>.Right(new Config(jsonDict));
    }

    // Implementation

    public delegate Option<A> Parser<A>(object node);

    public static readonly Parser<Dictionary<string, object>> jsClassParser =
      n => F.opt(n as Dictionary<string, object>);
    public static readonly Parser<object> objectParser = n => F.some(n);
    public static readonly Parser<string> stringParser = n => F.opt(n as string);
    public static Option<A> castA<A>(object a) { return a is A ? F.some((A) a) : F.none<A>(); }

    public static readonly Parser<int> intParser = n => {
      try {
        if (n is ulong) return F.some((int)(ulong)n);
        if (n is long) return F.some((int)(long)n);
        if (n is uint) return F.some((int)(uint)n);
        if (n is int) return F.some((int)n);
      }
      catch (OverflowException) {}
      return Option<int>.None;
    };
    public static readonly Parser<uint> uintParser = n => {
      try {
        if (n is ulong) return F.some((uint)(ulong)n);
        if (n is long) return F.some((uint)(long)n);
        if (n is uint) return F.some((uint)n);
        if (n is int) return F.some((uint)(int)n);
      }
      catch (OverflowException) {}
      return Option<uint>.None;
    };
    public static readonly Parser<long> longParser = n => {
      try {
        if (n is ulong) return F.some((long)(ulong)n);
        if (n is long) return F.some((long)n);
        if (n is uint) return F.some((long)(uint)n);
        if (n is int) return F.some((long)(int)n);
      }
      catch (OverflowException) {}
      return Option<long>.None;
    };
    public static readonly Parser<ulong> ulongParser = n => {
      try {
        if (n is ulong) return F.some((ulong) n);
        if (n is long) return F.some((ulong) (long) n);
        if (n is uint) return F.some((ulong) (uint) n);
        if (n is int) return F.some((ulong) (int) n);
      }
      catch (OverflowException) { }
      return Option<ulong>.None;
    };
    public static readonly Parser<float> floatParser = n => {
      try {
        if (n is double) return F.some((float) (double) n);
        if (n is float) return F.some((float) n);
        if (n is long) return F.some((float) (long) n);
        if (n is ulong) return F.some((float) (ulong) n);
        if (n is int) return F.some((float) (int) n);
        if (n is uint) return F.some((float) (uint) n);
      }
      catch (OverflowException) {}
      return Option<float>.None;
    };
    static readonly Fn<object, Option<float>> fnFloatParser = _ => floatParser(_);
    public static readonly Parser<double> doubleParser = n => {
      try {
        if (n is double) return F.some((double) n);
        if (n is float) return F.some((double) (float) n);
        if (n is long) return F.some((double) (long) n);
        if (n is ulong) return F.some((double) (ulong) n);
        if (n is int) return F.some((double) (int) n);
        if (n is uint) return F.some((double) (uint) n);
      }
      catch (OverflowException) { }
      return Option<double>.None;
    };
    public static readonly Parser<bool> boolParser = n => castA<bool>(n);

    public static readonly Parser<FRange> fRangeParser = n => {
      foreach (var dict in F.opt(n as Dictionary<string, object>)) {
        foreach (var lower in dict.get("lower").flatMap(fnFloatParser))
          foreach (var upper in dict.get("upper").flatMap(fnFloatParser))
            return F.some(new FRange(lower, upper));
      }
      return Option<FRange>.None;
    };

    public static readonly Parser<DateTime> dateTimeParser = n =>
      n is DateTime
      ? F.some((DateTime) n)
      : F.opt(n as string).flatMap(_ => _.parseDateTime().value);

    public override string scope { get; }

    readonly Dictionary<string, object> root, scopedRoot;

    public Config(Dictionary<string, object> root, Dictionary<string, object> scopedRoot=null, string scope="") {
      this.scope = scope;
      this.root = root;
      this.scopedRoot = scopedRoot ?? root;
    }

    #region either getters

    public override Either<Configuration.ConfigFetchError, object> eitherObject(string key)
    { return get(key, objectParser); }

    public override Either<Configuration.ConfigFetchError, string> eitherString(string key)
    { return get(key, stringParser); }

    public override Either<Configuration.ConfigFetchError, int> eitherInt(string key)
    { return get(key, intParser); }

    public override Either<Configuration.ConfigFetchError, uint> eitherUInt(string key)
    { return get(key, uintParser); }

    public override Either<Configuration.ConfigFetchError, long> eitherLong(string key)
    { return get(key, longParser); }

    public override Either<Configuration.ConfigFetchError, ulong> eitherULong(string key)
    { return get(key, ulongParser); }

    public override Either<Configuration.ConfigFetchError, float> eitherFloat(string key)
    { return get(key, floatParser); }

    public override Either<Configuration.ConfigFetchError, double> eitherDouble(string key)
    { return get(key, doubleParser); }

    public override Either<Configuration.ConfigFetchError, bool> eitherBool(string key)
    { return get(key, boolParser); }

    public override Either<Configuration.ConfigFetchError, FRange> eitherFRange(string key)
    { return get(key, fRangeParser); }

    public override Either<Configuration.ConfigFetchError, DateTime> eitherDateTime(string key)
    { return get(key, dateTimeParser); }

    public override Either<Configuration.ConfigFetchError, IConfig> eitherSubConfig(string key) {
      return get(key, jsClassParser).mapRight(n =>
        (IConfig)new Config(root, n, scope == "" ? key : scope + "." + key)
      );
    }

    public override Either<Configuration.ConfigFetchError, IList<IConfig>> eitherSubConfigList(string key) {
      return eitherList(key, jsClassParser).mapRight(nList => {
        var lst = F.emptyList<IConfig>(nList.Count);
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var idx = 0; idx < nList.Count; idx++) {
          var n = nList[idx];
          lst.Add(new Config(root, n, $"{(scope == "" ? key : scope + "." + key)}[{idx}]"));
        }
        return (IList<IConfig>)lst;
      });
    }

    public override Either<Configuration.ConfigFetchError, IList<A>> eitherList<A>(string key, Parser<A> parser) {
      return get(key, n => F.some(n as List<object>)).flatMapRight(arr => {
        var list = new List<A>(arr.Count);
        for (var idx = 0; idx < arr.Count; idx++) {
          var node = arr[idx];
          var parsed = parser(node);
          if (parsed.isDefined) list.Add(parsed.get);
          else return F.left<Configuration.ConfigFetchError, IList<A>>(Configuration.ConfigFetchError.wrongType(
            $"Cannot convert '{key}'[{idx}] to {typeof(A)}: {node}"
          ));
        }
        return F.right<Configuration.ConfigFetchError, IList<A>>(list);
      });
    }

    #endregion

    Either<Configuration.ConfigFetchError, A> get<A>(string key, Parser<A> parser, Dictionary<string, object> current = null) {
      var parts = split(key);

      current = current ?? scopedRoot;
      foreach (var part in parts.dropRight(1)) {
        var either = fetch(current, key, part, jsClassParser);
        if (either.isLeft) return either.mapRight(_ => default(A));
        current = either.rightValue.get;
      }

      return fetch(current, key, parts[parts.Length - 1], parser);
    }

    static string[] split(string key) {
      return key.Split('.');
    }

    Either<Configuration.ConfigFetchError, A> fetch<A>(
      Dictionary<string, object> current, string key, string part, Parser<A> parser
    ) {
      if (!current.ContainsKey(part))
        return F.left<Configuration.ConfigFetchError, A>(Configuration.ConfigFetchError.keyNotFound(
          $"Cannot find part '{part}' from key '{key}' in {current.asString()} " +
          $"[scope='{scope}']"
        ));
      var node = current[part];

      return followReference(node).flatMapRight(n =>
        parser(n).fold(
          () => F.left<Configuration.ConfigFetchError, A>(Configuration.ConfigFetchError.wrongType(
            $"Cannot convert part '{part}' from key '{key}' to {typeof (A)}. Type={n.GetType()}" +
            $" Contents: {n}"
          )), F.right<Configuration.ConfigFetchError, A>
        )
      );
    }

    Either<Configuration.ConfigFetchError, object> followReference(object current) {
      var str = current as string;
      // references are specified with '#REF=...#'
      if (
        str != null &&
        str.Length >= 6
        && str.Substring(0, 5) == "#REF="
        && str.Substring(str.Length - 1, 1) == "#"
      ) {
        var key = str.Substring(5, str.Length - 6);
        // References are always followed from the root tree.
        return get(key, F.some, root).mapLeft(err =>
          Configuration.ConfigFetchError.brokenRef($"While following reference {str}: {err}")
        );
      }
      else return F.right<Configuration.ConfigFetchError, object>(current);
    }

    public override string ToString() {
      return $"Config(scope: \"{scope}\", data: {scopedRoot})";
    }
  }

  public static class ConfigExts {
    public static Config.Parser<B> flatMap<A, B>(this Config.Parser<A> aParser, Fn<A, Option<B>> f) =>
      o => aParser(o).flatMap(f);
  }
}
