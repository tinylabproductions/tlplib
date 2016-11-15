using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Configuration {
  /** Representation of configuration path. */
  public struct ConfigPath {
    public const char SEPARATOR = '.';
    public static ConfigPath root = new ConfigPath(ImmutableList<string>.Empty);

    public readonly ImmutableList<string> path;
    public readonly Option<string> basedFrom;

    public ConfigPath(ImmutableList<string> path, Option<string> basedFrom) {
      this.path = path;
      this.basedFrom = basedFrom;
    }

    ConfigPath(ImmutableList<string> path) : this(path, Option<string>.None) {}

    public ConfigPath baseOn(ConfigPath basePath) => 
      new ConfigPath(path, basePath.pathStrWithBase.some());

    public bool isRoot => path.isEmpty();

    public string pathStr => path.mkString(SEPARATOR);

    public string pathStrWithBase { get {
      var basedS = basedFrom.isDefined ? $"({basedFrom.get})." : "";
      return $"{basedS}{pathStr}";
    } }

    public override string ToString() => $"{nameof(ConfigPath)}[{pathStrWithBase}]";

    public static ConfigPath operator /(ConfigPath s1, string s2) =>
      new ConfigPath(s1.path.AddRange(s2.Split(SEPARATOR)), s1.basedFrom);

    public ConfigPath indexed(int idx) => this / $"[{idx}]";

    public ConfigPath keyed(string key) => this / $"[key={key}]";
  }

  /* See IConfig. */
  public class Config : IConfig {
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

    #region Parsers

    /** 
     * Either Left(additional error message or "" if none) or Right(value).
     */
    public delegate Either<ConfigLookupError, A> Parser<A>(ConfigPath path, object node);

    public static ConfigLookupError parseErrorFor<A>(
      ConfigPath path, object node, string extraInfo = null
    ) {
      var extraS = extraInfo == null ? "" : $", {extraInfo}";
      return ConfigLookupError.wrongType(F.lazy(() =>
        $"Can't parse {path} as {typeof(A)}{extraS}, node contents: {node.asString()}"
      ));
    }

    public static Either<ConfigLookupError, A> parseErrorEFor<A>(
      ConfigPath path, object node, string extraInfo = null
    ) => Either<ConfigLookupError, A>.Left(parseErrorFor<A>(path, node, extraInfo));

    public static Parser<A> createCastParser<A>() => (path, node) => 
      node is A
      ? Either<ConfigLookupError, A>.Right((A) node) 
      : parseErrorEFor<A>(path, node);

    public static readonly Parser<object> objectParser = (_, n) =>
      Either<ConfigLookupError, object>.Right(n);

    public static readonly Parser<List<object>> objectListParser = createCastParser<List<object>>();

    public static Parser<List<A>> listParser<A>(Parser<A> parser) =>
      objectListParser.flatMap((path, objList) => {
        var list = new List<A>(objList.Count);
        for (var idx = 0; idx < objList.Count; idx++) {
          var idxPath = path.indexed(idx);
          var parsedE = parser(idxPath, objList[idx]);
          if (parsedE.isLeft)
            return Either<ConfigLookupError, List<A>>.Left(parsedE.__unsafeGetLeft);
          list.Add(parsedE.__unsafeGetRight);
        }
        return Either<ConfigLookupError, List<A>>.Right(list);
      });

    public static readonly Parser<Dictionary<string, object>> jsClassParser =
      createCastParser<Dictionary<string, object>>();

    public static readonly Parser<IConfig> configParser =
      jsClassParser.map((path, dict) => (IConfig) new Config(dict, ConfigPath.root.baseOn(path)));

    public static Parser<Dictionary<K, V>> dictParser<K, V>(
      Parser<K> keyParser, Parser<V> valueParser
    ) =>
      configParser.flatMap((path, cfg) => {
        var keys = cfg.keys;
        var dict = new Dictionary<K, V>(keys.Count);
        foreach (var dictKey in keys) {
          var keyPath = path.keyed(dictKey);
          var parsedKeyE = keyParser(keyPath, dictKey);
          if (parsedKeyE.isLeft)
            return new Either<ConfigLookupError, Dictionary<K, V>>(parsedKeyE.__unsafeGetLeft);
          var parsedKey = parsedKeyE.__unsafeGetRight;
          if (dict.ContainsKey(parsedKey))
            return parseErrorEFor<Dictionary<K, V>>(
              keyPath, dictKey, $"key already exists as '{dict[parsedKey]}'"
            );

          var parsedValE = valueParser(path / dictKey, cfg.getObject(dictKey));
          if (parsedValE.isLeft)
            return new Either<ConfigLookupError, Dictionary<K, V>>(parsedValE.__unsafeGetLeft);

          dict.Add(parsedKey, parsedValE.__unsafeGetRight);
        }
        return Either<ConfigLookupError, Dictionary<K, V>>.Right(dict);
      });

    public static readonly Parser<FRange> fRangeParser =
      configParser.flatMap((path, cfg) => { 
        var lowerE = cfg.eitherGet("lower", floatParser);
        if (lowerE.isLeft) return lowerE.__unsafeCastRight<FRange>();
        var upperE = cfg.eitherGet("upper", floatParser);
        if (upperE.isLeft) return upperE.__unsafeCastRight<FRange>();
        return Either<ConfigLookupError, FRange>.Right(
          new FRange(lowerE.__unsafeGetRight, upperE.__unsafeGetRight)
        );
      });

    public static readonly Parser<string> stringParser = createCastParser<string>();

    public static readonly Parser<int> intParser = (path, n) => {
      try {
        if (n is ulong) return Either<ConfigLookupError, int>.Right((int)(ulong)n);
        if (n is long) return Either<ConfigLookupError, int>.Right((int)(long)n);
        if (n is uint) return Either<ConfigLookupError, int>.Right((int)(uint)n);
        if (n is int) return Either<ConfigLookupError, int>.Right((int)n);
      }
      catch (OverflowException) {}
      return parseErrorEFor<int>(path, n);
    };

    public static readonly Parser<uint> uintParser = (path, n) => {
      try {
        if (n is ulong) return Either<ConfigLookupError, uint>.Right((uint)(ulong)n);
        if (n is long) return Either<ConfigLookupError, uint>.Right((uint)(long)n);
        if (n is uint) return Either<ConfigLookupError, uint>.Right((uint)n);
        if (n is int) return Either<ConfigLookupError, uint>.Right((uint)(int)n);
      }
      catch (OverflowException) {}
      return parseErrorEFor<uint>(path, n);
    };

    public static readonly Parser<long> longParser = (path, n) => {
      try {
        if (n is ulong) return Either<ConfigLookupError, long>.Right((long)(ulong)n);
        if (n is long) return Either<ConfigLookupError, long>.Right((long)n);
        if (n is uint) return Either<ConfigLookupError, long>.Right((uint)n);
        if (n is int) return Either<ConfigLookupError, long>.Right((int)n);
      }
      catch (OverflowException) {}
      return parseErrorEFor<long>(path, n);
    };

    public static readonly Parser<ulong> ulongParser = (path, n) => {
      try {
        if (n is ulong) return Either<ConfigLookupError, ulong>.Right((ulong) n);
        if (n is long) return Either<ConfigLookupError, ulong>.Right((ulong) (long) n);
        if (n is uint) return Either<ConfigLookupError, ulong>.Right((uint) n);
        if (n is int) return Either<ConfigLookupError, ulong>.Right((ulong) (int) n);
      }
      catch (OverflowException) { }
      return parseErrorEFor<ulong>(path, n);
    };

    public static readonly Parser<float> floatParser = (path, n) => {
      try {
        if (n is double) return Either<ConfigLookupError, float>.Right((float) (double) n);
        if (n is float) return Either<ConfigLookupError, float>.Right((float) n);
        if (n is long) return Either<ConfigLookupError, float>.Right((long) n);
        if (n is ulong) return Either<ConfigLookupError, float>.Right((ulong) n);
        if (n is int) return Either<ConfigLookupError, float>.Right((int) n);
        if (n is uint) return Either<ConfigLookupError, float>.Right((uint) n);
      }
      catch (OverflowException) {}
      return parseErrorEFor<float>(path, n);
    };

    public static readonly Parser<double> doubleParser = (path, n) => {
      try {
        if (n is double) return Either<ConfigLookupError, double>.Right((double) n);
        if (n is float) return Either<ConfigLookupError, double>.Right((float) n);
        if (n is long) return Either<ConfigLookupError, double>.Right((long) n);
        if (n is ulong) return Either<ConfigLookupError, double>.Right((ulong) n);
        if (n is int) return Either<ConfigLookupError, double>.Right((int) n);
        if (n is uint) return Either<ConfigLookupError, double>.Right((uint) n);
      }
      catch (OverflowException) { }
      return parseErrorEFor<double>(path, n);
    };

    public static readonly Parser<bool> boolParser = createCastParser<bool>();

    public static readonly Parser<DateTime> dateTimeParser = 
      createCastParser<DateTime>()
      .or(stringParser.flatMap((path, s) => {
        var t = s.parseDateTime();
        return t.isSuccess 
          ? Either<ConfigLookupError, DateTime>.Right(t.__unsafeGet) 
          : parseErrorEFor<DateTime>(path, s, t.__unsafeException.Message);
      }));

    #endregion

    public ConfigPath scope { get; }

    readonly Dictionary<string, object> root;

    public Config(
      Dictionary<string, object> root, ConfigPath scope
    ) {
      this.scope = scope;
      this.root = root;
    }

    public Config(Dictionary<string, object> root) : this(root, ConfigPath.root) {}

    public ICollection<string> keys => root.Keys;

    #region Getters

    public A as_<A>(Parser<A> parser) =>
      e2a(eitherAs(parser));

    public A get<A>(string key, Parser<A> parser) => 
      e2a(internalGet(key, parser));

    static A e2a<A>(Either<ConfigLookupError, A> e) {
      if (e.isLeft) throw new ConfigFetchException(e.__unsafeGetLeft);
      return e.__unsafeGetRight;
    }

    public Option<A> optAs<A>(Parser<A> parser) => 
      eitherAs(parser).rightValue;

    public Option<A> optGet<A>(string key, Parser<A> parser) =>
      internalGet(key, parser).rightValue;

    public Try<A> tryAs<A>(Parser<A> parser) => 
      e2t(eitherAs(parser));

    public Try<A> tryGet<A>(string key, Parser<A> parser) => 
      e2t(internalGet(key, parser));

    static Try<A> e2t<A>(Either<ConfigLookupError, A> e) =>
      e.isLeft
        ? new Try<A>(new ConfigFetchException(e.__unsafeGetLeft))
        : new Try<A>(e.__unsafeGetRight);

    public Either<ConfigLookupError, A> eitherAs<A>(Parser<A> parser) => 
      parser(scope, root);

    public Either<ConfigLookupError, A> eitherGet<A>(
      string key, Parser<A> parser
    ) => internalGet(key, parser);
    
    #endregion



    Either<ConfigLookupError, A> internalGet<A>(
      string key, Parser<A> parser, Dictionary<string, object> current = null
    ) {
      var path = scope / key;
      var parts = path.path;

      current = current ?? root;
      var toIdx = parts.Count - 1;
      for (var idx = 0; idx < toIdx; idx++) {
        var idxPart = parts[idx];
        var either = fetch(current, path, idxPart, jsClassParser);
        if (either.isLeft) return either.__unsafeCastRight<A>();
        current = either.rightValue.get;
      }

      return fetch(current, path, parts[toIdx], parser);
    }

    Either<ConfigLookupError, A> fetch<A>(
      IDictionary<string, object> current, ConfigPath path, string part, Parser<A> parser
    ) {
      if (!current.ContainsKey(part))
        return F.left<ConfigLookupError, A>(ConfigLookupError.keyNotFound(F.lazy(() =>
          $"Cannot find part '{part}' from path '{path}' in {current.asString()} " +
          $"[{nameof(scope)}='{scope}']"
        )));

      var node = current[part];
      return parser(path, node);
    }

    public override string ToString() => 
      $"{nameof(Config)}({nameof(scope)}: \"{scope}\", {nameof(root)}: {root})";
  }

  public static class ConfigExts {
    public static Config.Parser<B> map<A, B>(this Config.Parser<A> aParser, Fn<ConfigPath, A, B> f) =>
      (path, o) => aParser(path, o).mapRight(a => f(path, a));

    public static Config.Parser<B> flatMap<A, B>(
      this Config.Parser<A> aParser, Fn<ConfigPath, A, Option<B>> f
    ) => aParser.flatMap((path, a) => {
      var bOpt = f(path, a);
      return bOpt.isDefined
        ? Either<ConfigLookupError, B>.Right(bOpt.get) 
        : Config.parseErrorEFor<B>(path, a);
    });

    public static Config.Parser<B> flatMap<A, B>(
      this Config.Parser<A> aParser, Fn<ConfigPath, A, Either<ConfigLookupError, B>> f
    ) =>
      (path, o) => aParser(path, o).flatMapRight(a => f(path, a));

    public static Config.Parser<B> flatMapTry<A, B>(
      this Config.Parser<A> aParser, Fn<ConfigPath, A, B> f
    ) => 
      (path, o) => aParser(path, o).flatMapRight(a => {
        try { return new Either<ConfigLookupError, B>(f(path, a)); }
        catch (ConfigFetchException e) { return new Either<ConfigLookupError, B>(e.error); }
      });

    public static Config.Parser<A> filter<A>(
      this Config.Parser<A> parser, Fn<A, bool> predicate
    ) =>
      (path, o) => parser(path, o).flatMapRight(a => 
        predicate(a) 
        ? new Either<ConfigLookupError, A>(a) 
        : Config.parseErrorEFor<A>(path, a, "didn't pass predicate")
      );

    public static Config.Parser<B> collect<A, B>(
      this Config.Parser<A> parser, Fn<A, Option<B>> collector
    ) =>
      (path, o) => parser(path, o).flatMapRight(a => {
        var bOpt = collector(a);
        return bOpt.isDefined
          ? new Either<ConfigLookupError, B>(bOpt.get)
          : Config.parseErrorEFor<B>(path, a, "didn't pass collector");
      });

    public static Config.Parser<A> or<A>(this Config.Parser<A> a1, Config.Parser<A> a2) =>
      (path, node) => {
        var a1E = a1(path, node);
        return a1E.isRight ? a1E : a2(path, node);
      };

    public static Config.Parser<Tpl<A1, A2>> and<A1, A2>(
      this Config.Parser<A1> a1p, Config.Parser<A2> a2p
    ) =>
      (path, node) => {
        var a1E = a1p(path, node);
        if (a1E.isLeft) return a1E.__unsafeCastRight<Tpl<A1, A2>>();
        var a2E = a2p(path, node);
        if (a2E.isLeft) return a2E.__unsafeCastRight<Tpl<A1, A2>>();
        return new Either<ConfigLookupError, Tpl<A1, A2>>(F.t(
          a1E.__unsafeGetRight, a2E.__unsafeGetRight
        ));
      };

    public static Config.Parser<Tpl<A1, A2, A3>> and<A1, A2, A3>(
        this Config.Parser<A1> a1p, Config.Parser<A2> a2p, Config.Parser<A3> a3p
      ) =>
      (path, node) => {
        var a1E = a1p(path, node);
        if (a1E.isLeft) return a1E.__unsafeCastRight<Tpl<A1, A2, A3>>();
        var a2E = a2p(path, node);
        if (a2E.isLeft) return a2E.__unsafeCastRight<Tpl<A1, A2, A3>>();
        var a3E = a3p(path, node);
        if (a3E.isLeft) return a3E.__unsafeCastRight<Tpl<A1, A2, A3>>();
        return new Either<ConfigLookupError, Tpl<A1, A2, A3>>(F.t(
          a1E.__unsafeGetRight, a2E.__unsafeGetRight, a3E.__unsafeGetRight
        ));
      };

       public static Config.Parser<Tpl<A1, A2, A3, A4>> and<A1, A2, A3, A4>(
      this Config.Parser<A1> a1p, Config.Parser<A2> a2p, Config.Parser<A3> a3p, Config.Parser<A4> a4p
    ) =>
      (path, node) => {
        var a1E = a1p(path, node);
        if (a1E.isLeft) return a1E.__unsafeCastRight<Tpl<A1, A2, A3, A4>>();
        var a2E = a2p(path, node);
        if (a2E.isLeft) return a2E.__unsafeCastRight<Tpl<A1, A2, A3, A4>>();
        var a3E = a3p(path, node);
        if (a3E.isLeft) return a3E.__unsafeCastRight<Tpl<A1, A2, A3, A4>>();
        var a4E = a4p(path, node);
        if (a4E.isLeft) return a4E.__unsafeCastRight<Tpl<A1, A2, A3, A4>>();
        return new Either<ConfigLookupError, Tpl<A1, A2, A3, A4>>(F.t(
          a1E.__unsafeGetRight, a2E.__unsafeGetRight, a3E.__unsafeGetRight, a4E.__unsafeGetRight
        ));
      };
  }
}
