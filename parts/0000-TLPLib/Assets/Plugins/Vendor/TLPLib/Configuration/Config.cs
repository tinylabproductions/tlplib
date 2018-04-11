using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;

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
      var basedS = basedFrom.isSome ? $"({basedFrom.get})." : "";
      return $"{basedS}{pathStr}";
    } }

    public override string ToString() => $"{nameof(ConfigPath)}[{pathStrWithBase}]";

    public static ConfigPath operator /(ConfigPath s1, string s2) =>
      new ConfigPath(s1.path.AddRange(s2.Split(SEPARATOR)), s1.basedFrom);

    public ConfigPath indexed(int idx) => this / $"[{idx}]";

    public ConfigPath keyed(string key) => this / $"[key={key}]";
  }

  /* See IConfig. */
  public partial class Config : IConfig {
    [Record]
    public partial struct ParsingError {
      public readonly Option<Exception> exception;
      public readonly string jsonString;

      public ImmutableArray<Tpl<string, string>> getExtras() => ImmutableArray.Create(
        F.t("json", jsonString),
        F.t("exception", exception.ToString())
      );
    }

    public static Either<ParsingError, IConfig> parseJson(string json) {
      if (json.isEmpty(trim: true)) 
        return new ParsingError(Option<Exception>.None, $"<empty>('{json}')");
      
      try {
        var jsonDict = (Dictionary<string, object>) Json.Deserialize(json);
        return jsonDict == null
          ? Either<ParsingError, IConfig>.Left(new ParsingError(Option<Exception>.None, json))
          : Either<ParsingError, IConfig>.Right(new Config(jsonDict));
      }
      catch (Exception e) {
        return new ParsingError(e.some(), json);
      }
    }

    // Implementation

    #region Parsers

    /**
     * Either Left(additional error message or "" if none) or Right(value).
     */
    public delegate Either<ConfigLookupError, A> Parser<A>(ConfigPath path, object node);

    public static ConfigLookupError parseErrorFor<A>(
      ConfigPath path, object node, string extraInfo = null
    ) =>
      ConfigLookupError.wrongType(F.lazy(() => ImmutableArray.Create(
        F.t("path", path.pathStrWithBase),
        F.t("type", typeof(A).FullName),
        F.t("extraInfo", extraInfo ?? ""),
        F.t("node-contents", node.asDebugString())
      )));

    public static Either<ConfigLookupError, A> parseErrorEFor<A>(
      ConfigPath path, object node, string extraInfo = null
    ) => Either<ConfigLookupError, A>.Left(parseErrorFor<A>(path, node, extraInfo));

    public static Parser<A> createCastParser<A>() => (path, node) =>
      node is A
      ? Either<ConfigLookupError, A>.Right((A) node)
      : parseErrorEFor<A>(path, node);

    /** Parser that always succeeds and returns constant. */
    public static Parser<A> constantParser<A>(A a) =>
      (path, _) => Either<ConfigLookupError, A>.Right(a);

    public static readonly Parser<object> objectParser = (_, n) =>
      Either<ConfigLookupError, object>.Right(n);

    public static readonly Parser<List<object>> objectListParser = createCastParser<List<object>>();

    public static Parser<Option<A>> opt<A>(Parser<A> parser) =>
      (path, o) =>
        o == null
        ? Either<ConfigLookupError, Option<A>>.Right(Option<A>.None)
        : parser(path, o).mapRight(_ => _.some());

    public static Parser<CB> collectionParser<CB, A>(
      Parser<A> parser,
      Fn<int, CB> createCollectionBuilder,
      Fn<CB, A, CB> add
    ) =>
      objectListParser.flatMap((path, objList) => {
        var builder = createCollectionBuilder(objList.Count);
        for (var idx = 0; idx < objList.Count; idx++) {
          var idxPath = path.indexed(idx);
          var parsedE = parser(idxPath, objList[idx]);
          if (parsedE.isLeft)
            return Either<ConfigLookupError, CB>.Left(parsedE.__unsafeGetLeft);
          builder = add(builder, parsedE.__unsafeGetRight);
        }
        return Either<ConfigLookupError, CB>.Right(builder);
      });

    public static Parser<List<A>> listParser<A>(Parser<A> parser) =>
      collectionParser(parser, count => new List<A>(count), (l, a) => {
        l.Add(a);
        return l;
      });

    public static Parser<ImmutableArray<A>> immutableArrayParser<A>(Parser<A> parser) =>
      collectionParser(parser, ImmutableArray.CreateBuilder<A>, (b, a) => {
        b.Add(a);
        return b;
      }).map(_ => _.MoveToImmutable());

    public static Parser<ImmutableList<A>> immutableListParser<A>(Parser<A> parser) =>
      collectionParser(parser, count => ImmutableList.CreateBuilder<A>(), (b, a) => {
        b.Add(a);
        return b;
      }).map(_ => _.ToImmutable());

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

    public static Parser<A> configPathedParser<A>(string key, Parser<A> aParser) =>
      configParser.flatMap((path, cfg) => cfg.eitherGet(key, aParser));

    public static Parser<B> configPathedParser<A1, A2, B>(
      string a1Key, Parser<A1> a1Parser,
      string a2Key, Parser<A2> a2Parser,
      Fn<A1, A2, B> mapper
    ) =>
      configPathedParser(a1Key, a1Parser)
      .and(configPathedParser(a2Key, a2Parser))
      .map((path, t) => mapper(t._1, t._2));

    public static Parser<B> configPathedParser<A1, A2, A3, B>(
      string a1Key, Parser<A1> a1Parser,
      string a2Key, Parser<A2> a2Parser,
      string a3Key, Parser<A3> a3Parser,
      Fn<A1, A2, A3, B> mapper
    ) =>
      configPathedParser(a1Key, a1Parser)
      .and(
        configPathedParser(a2Key, a2Parser),
        configPathedParser(a3Key, a3Parser)
      )
      .map((path, t) => mapper(t._1, t._2, t._3));

    public static Parser<B> configPathedParser<A1, A2, A3, A4, B>(
      string a1Key, Parser<A1> a1Parser,
      string a2Key, Parser<A2> a2Parser,
      string a3Key, Parser<A3> a3Parser,
      string a4Key, Parser<A4> a4Parser,
      Fn<A1, A2, A3, A4, B> mapper
    ) =>
      configPathedParser(a1Key, a1Parser)
      .and(
        configPathedParser(a2Key, a2Parser),
        configPathedParser(a3Key, a3Parser),
        configPathedParser(a4Key, a4Parser)
      )
      .map((path, t) => mapper(t._1, t._2, t._3, t._4));

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

    public static Parser<R> rangeParser<A, R>(Parser<A> aParser, Fn<A, A, R> lowerUpperToRange) =>
      configPathedParser("lower", aParser)
      .and(configPathedParser("upper", aParser))
      .map((path, t) => lowerUpperToRange(t._1, t._2));

    public static readonly Parser<Range> iRangeParser =
      rangeParser(intParser, (l, u) => new Range(l, u));

    public static readonly Parser<FRange> fRangeParser =
      rangeParser(floatParser, (l, u) => new FRange(l, u));

    public static readonly Parser<URange> uRangeParser =
      rangeParser(uintParser, (l, u) => new URange(l, u));

    public static readonly Parser<Url>
      urlParser = stringParser.map(s => new Url(s)),
      /** for relative paths, like 'foo bar/baz.jpg' */
      uriEscapedUrlParser = urlParser.map(url => new Url(Uri.EscapeUriString(url.url)));

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
        return F.left<ConfigLookupError, A>(ConfigLookupError.keyNotFound(F.lazy(() => ImmutableArray.Create(
          F.t(nameof(part), part),
          F.t(nameof(path), path.pathStrWithBase),
          F.t(nameof(current), current.asDebugString()),
          F.t(nameof(scope), scope.pathStrWithBase)
        ))));

      var node = current[part];
      return parser(path, node);
    }

    public override string ToString() =>
      $"{nameof(Config)}({nameof(scope)}: \"{scope}\", {nameof(root)}: {root})";
  }

  public static class ConfigExts {
    public static Config.Parser<B> map<A, B>(this Config.Parser<A> aParser, Fn<ConfigPath, A, B> f) =>
      (path, o) => aParser(path, o).mapRight(a => f(path, a));

    public static Config.Parser<B> map<A, B>(this Config.Parser<A> aParser, Fn<A, B> f) =>
      aParser.map((path, a) => f(a));

    public static Config.Parser<B> flatMap<A, B>(
      this Config.Parser<A> aParser, Fn<ConfigPath, A, Option<B>> f
    ) => aParser.flatMap((path, a) => {
      var bOpt = f(path, a);
      return bOpt.isSome
        ? Either<ConfigLookupError, B>.Right(bOpt.get)
        : Config.parseErrorEFor<B>(path, a);
    });

    public static Config.Parser<B> flatMap<A, B>(
      this Config.Parser<A> aParser, Fn<A, Option<B>> f
    ) => aParser.flatMap((path, a) => f(a));

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
        return bOpt.isSome
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
