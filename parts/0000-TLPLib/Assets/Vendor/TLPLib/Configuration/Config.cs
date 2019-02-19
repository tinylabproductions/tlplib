using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;

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
        return 
          from jsonDict in Json.Deserialize(json).cast().toE<Dictionary<string, object>>().mapLeft(err =>
            new ParsingError(F.some(new Exception($"Config root must be a JSON object, but: {err}")), json)
          )
          from res in jsonDict == null
            ? Either<ParsingError, IConfig>.Left(new ParsingError(Option<Exception>.None, json))
            : Either<ParsingError, IConfig>.Right(new Config(jsonDict))
          select res;
      }
      catch (Exception e) {
        return new ParsingError(e.some(), json);
      }
    }

    // Implementation

    #region Parsers

    public delegate Either<ConfigLookupError, To> Parser<in From, To>(ConfigPath path, From node);

    public static ConfigLookupError parseErrorFor<A>(
      ConfigPath path, object node, string extraInfo = null
    ) =>
      ConfigLookupError.wrongType(F.lazy(() => ImmutableArray.Create(
        F.t("path", path.pathStrWithBase),
        F.t("expected-type", typeof(A).FullName),
        F.t("actual-type", node.GetType().FullName),
        F.t("extraInfo", extraInfo ?? ""),
        F.t("node-contents", node.asDebugString())
      )));

    public static Either<ConfigLookupError, A> parseErrorEFor<A>(
      ConfigPath path, object node, string extraInfo = null
    ) => Either<ConfigLookupError, A>.Left(parseErrorFor<A>(path, node, extraInfo));

    public static Parser<object, A> createCastParser<A>() => (path, node) =>
      node is A a
      ? Either<ConfigLookupError, A>.Right(a)
      : parseErrorEFor<A>(path, node);

    /// Parser that always succeeds and returns constant.
    public static Parser<object, A> constantParser<A>(A a) =>
      (path, _) => Either<ConfigLookupError, A>.Right(a);

    public static readonly Parser<object, object> objectParser = (_, n) =>
      Either<ConfigLookupError, object>.Right(n);

    public static readonly Parser<object, List<object>> objectListParser = 
      createCastParser<List<object>>();

    public static Parser<From, Option<A>> opt<From, A>(Parser<From, A> parser) =>
      (path, o) =>
        o == null
        ? Either<ConfigLookupError, Option<A>>.Right(Option<A>.None)
        : parser(path, o).mapRight(_ => _.some());

    public static Parser<object, CB> collectionParser<CB, A>(
      Parser<object, A> parser,
      Func<int, CB> createCollectionBuilder,
      Func<CB, A, CB> add
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

    public static Parser<object, List<A>> listParser<A>(Parser<object, A> parser) =>
      collectionParser(parser, count => new List<A>(count), (l, a) => {
        l.Add(a);
        return l;
      });

    public static Parser<object, ImmutableArray<A>> immutableArrayParser<A>(Parser<object, A> parser) =>
      collectionParser(parser, ImmutableArray.CreateBuilder<A>, (b, a) => {
        b.Add(a);
        return b;
      }).map(_ => _.MoveToImmutable());

    public static Parser<object, ImmutableList<A>> immutableListParser<A>(Parser<object, A> parser) =>
      collectionParser(parser, count => ImmutableList.CreateBuilder<A>(), (b, a) => {
        b.Add(a);
        return b;
      }).map(_ => _.ToImmutable());

    public static readonly Parser<object, Dictionary<string, object>> jsClassParser =
      createCastParser<Dictionary<string, object>>();

    public static readonly Parser<object, IConfig> configParser =
      jsClassParser.map((path, dict) => (IConfig) new Config(dict, ConfigPath.root.baseOn(path)));

    public static Parser<object, Dictionary<K, V>> dictParser<K, V>(
      Parser<string, K> keyParser, Parser<object, V> valueParser
    ) =>
      jsClassParser.flatMap((path, untypedDict) => {
        var dict = new Dictionary<K, V>(untypedDict.Count);
        foreach (var kv in untypedDict) {
          var key = kv.Key;
          var parsedKeyE = keyParser(path, key);
          {
            if (parsedKeyE.leftValueOut(out var err)) return err;
          }
          var parsedKey = parsedKeyE.__unsafeGetRight;

          if (dict.ContainsKey(parsedKey))
            return parseErrorEFor<Dictionary<K, V>>(
              path, kv.Key, $"key already exists as '{dict[parsedKey]}'"
            );

          var parsedValE = valueParser(path / key, kv.Value);
          if (parsedValE.isLeft)
            return new Either<ConfigLookupError, Dictionary<K, V>>(parsedValE.__unsafeGetLeft);

          dict.Add(parsedKey, parsedValE.__unsafeGetRight);
        }
        return Either<ConfigLookupError, Dictionary<K, V>>.Right(dict);
      });

    public static Parser<object, A> configPathedParser<A>(string key, Parser<object, A> aParser) =>
      configParser.flatMap((path, cfg) => cfg.eitherGet(key, aParser));

    public static Parser<object, B> configPathedParser<A1, A2, B>(
      string a1Key, Parser<object, A1> a1Parser,
      string a2Key, Parser<object, A2> a2Parser,
      Func<A1, A2, B> mapper
    ) =>
      configPathedParser(a1Key, a1Parser)
      .and(configPathedParser(a2Key, a2Parser))
      .map((path, t) => mapper(t._1, t._2));

    public static Parser<object, B> configPathedParser<A1, A2, A3, B>(
      string a1Key, Parser<object, A1> a1Parser,
      string a2Key, Parser<object, A2> a2Parser,
      string a3Key, Parser<object, A3> a3Parser,
      Func<A1, A2, A3, B> mapper
    ) =>
      configPathedParser(a1Key, a1Parser)
      .and(
        configPathedParser(a2Key, a2Parser),
        configPathedParser(a3Key, a3Parser)
      )
      .map((path, t) => mapper(t._1, t._2, t._3));

    public static Parser<object, B> configPathedParser<A1, A2, A3, A4, B>(
      string a1Key, Parser<object, A1> a1Parser,
      string a2Key, Parser<object, A2> a2Parser,
      string a3Key, Parser<object, A3> a3Parser,
      string a4Key, Parser<object, A4> a4Parser,
      Func<A1, A2, A3, A4, B> mapper
    ) =>
      configPathedParser(a1Key, a1Parser)
      .and(
        configPathedParser(a2Key, a2Parser),
        configPathedParser(a3Key, a3Parser),
        configPathedParser(a4Key, a4Parser)
      )
      .map((path, t) => mapper(t._1, t._2, t._3, t._4));

    public static Parser<A, A> idParser<A>() => (path, a) => a; 
    [PublicAPI] public static readonly Parser<object, string> stringParser = createCastParser<string>();
    [PublicAPI] public static readonly Parser<object, Guid> guidParser = 
      stringParser.flatMapTry((_, s) => new Guid(s));

    [PublicAPI] public static readonly Parser<object, int> intParser = (path, n) => {
      try {
        switch (n) {
          case ulong i: return (int)i;
          case long l: return (int)l;
          case uint u: return (int)u;
          case int i1: return i1;
        }
      }
      catch (OverflowException) {}
      return parseErrorEFor<int>(path, n);
    };

    [PublicAPI] public static Parser<object, byte> byteParser =
      intParser.flatMap(i => i < 0 || i > byte.MaxValue ? F.none_ : F.some((byte) i)); 

    [PublicAPI] public static readonly Parser<object, ushort> ushortParser = (path, n) => {
      try {
        switch (n) {
          case ulong u: return (ushort)u;
          case long l: return (ushort)l;
          case uint u1: return (ushort) u1;
          case int i: return (ushort)i;
          case ushort i: return i;
        }
      }
      catch (OverflowException) {}
      return parseErrorEFor<ushort>(path, n);
    };

    [PublicAPI]
    public static readonly Parser<object, uint> uintParser = (path, n) => {
      try {
        switch (n) {
          case ulong u: return (uint)u;
          case long l: return (uint)l;
          case uint u1: return u1;
          case int i: return (uint)i;
        }
      }
      catch (OverflowException) {}
      return parseErrorEFor<uint>(path, n);
    };

    [PublicAPI]
    public static readonly Parser<object, long> longParser = (path, n) => {
      try {
        switch (n) {
          case ulong l: return (long)l;
          case long l1: return l1;
          case uint u: return u;
          case int i: return i;
        }
      }
      catch (OverflowException) {}
      return parseErrorEFor<long>(path, n);
    };

    [PublicAPI]
    public static readonly Parser<object, ulong> ulongParser = (path, n) => {
      try {
        switch (n) {
          case ulong _: return (ulong) n;
          case long _: return (ulong) (long) n;
          case uint _: return (uint) n;
          case int _: return (ulong) (int) n;
        }
      }
      catch (OverflowException) { }
      return parseErrorEFor<ulong>(path, n);
    };

    [PublicAPI]
    public static readonly Parser<object, float> floatParser = (path, n) => {
      try {
        switch (n) {
          case double _: return (float) (double) n;
          case float _: return (float) n;
          case long _: return (long) n;
          case ulong _: return (ulong) n;
          case int _: return (int) n;
          case uint _: return (uint) n;
        }
      }
      catch (OverflowException) {}
      return parseErrorEFor<float>(path, n);
    };

    [PublicAPI]
    public static readonly Parser<object, double> doubleParser = (path, n) => {
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

    [PublicAPI]
    public static readonly Parser<object, bool> boolParser = createCastParser<bool>();

    public static readonly Parser<object, DateTime> dateTimeParser =
      createCastParser<DateTime>()
      .or(stringParser.flatMap((path, s) => {
        var t = s.parseDateTime();
        return t.isSuccess
          ? Either<ConfigLookupError, DateTime>.Right(t.__unsafeGet)
          : parseErrorEFor<DateTime>(path, s, t.__unsafeException.Message);
      }));

    public static Parser<object, R> rangeParser<A, R>(Parser<object, A> aParser, Func<A, A, R> lowerUpperToRange) =>
      configPathedParser("lower", aParser)
      .and(configPathedParser("upper", aParser))
      .map((path, t) => lowerUpperToRange(t._1, t._2));

    public static readonly Parser<object, Range> iRangeParser =
      rangeParser(intParser, (l, u) => new Range(l, u));

    public static readonly Parser<object, FRange> fRangeParser =
      rangeParser(floatParser, (l, u) => new FRange(l, u));

    public static readonly Parser<object, URange> uRangeParser =
      rangeParser(uintParser, (l, u) => new URange(l, u));

    public static readonly Parser<object, Url>
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

    public A as_<A>(Parser<object, A> parser) =>
      e2a(eitherAs(parser));

    public A get<A>(string key, Parser<object, A> parser) =>
      e2a(internalGet(key, parser));

    static A e2a<A>(Either<ConfigLookupError, A> e) {
      if (e.isLeft) throw new ConfigFetchException(e.__unsafeGetLeft);
      return e.__unsafeGetRight;
    }

    public Option<A> optAs<A>(Parser<object, A> parser) =>
      eitherAs(parser).rightValue;

    public Option<A> optGet<A>(string key, Parser<object, A> parser) =>
      internalGet(key, parser).rightValue;

    public Try<A> tryAs<A>(Parser<object, A> parser) =>
      e2t(eitherAs(parser));

    public Try<A> tryGet<A>(string key, Parser<object, A> parser) =>
      e2t(internalGet(key, parser));

    static Try<A> e2t<A>(Either<ConfigLookupError, A> e) =>
      e.isLeft
        ? new Try<A>(new ConfigFetchException(e.__unsafeGetLeft))
        : new Try<A>(e.__unsafeGetRight);

    public Either<ConfigLookupError, A> eitherAs<A>(Parser<object, A> parser) =>
      parser(scope, root);

    public Either<ConfigLookupError, A> eitherGet<A>(
      string key, Parser<object, A> parser
    ) => internalGet(key, parser);

    #endregion



    Either<ConfigLookupError, A> internalGet<A>(
      string key, Parser<object, A> parser, Dictionary<string, object> current = null
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
      IDictionary<string, object> current, ConfigPath path, string part, Parser<object, A> parser
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
    [PublicAPI] public static Config.Parser<From, B> map<From, A, B>(
      this Config.Parser<From, A> aParser, Func<ConfigPath, A, B> f
    ) =>
      (path, o) => aParser(path, o).mapRight(a => f(path, a));

    [PublicAPI] public static Config.Parser<From, B> map<From, A, B>(
      this Config.Parser<From, A> aParser, Func<A, B> f
    ) =>
      aParser.map((path, a) => f(a));

    [PublicAPI] public static Config.Parser<From, B> flatMap<From, A, B>(
      this Config.Parser<From, A> aParser, Func<ConfigPath, A, Option<B>> f
    ) => aParser.flatMap((path, a) => {
      var bOpt = f(path, a);
      return bOpt.isSome
        ? Either<ConfigLookupError, B>.Right(bOpt.get)
        : Config.parseErrorEFor<B>(path, a);
    });

    [PublicAPI] public static Config.Parser<From, B> flatMap<From, A, B>(
      this Config.Parser<From, A> aParser, Func<A, Option<B>> f
    ) => aParser.flatMap((path, a) => f(a));

    [PublicAPI] public static Config.Parser<From, B> flatMapParser<From, A, B>(
      this Config.Parser<From, A> aParser, Config.Parser<A, B> bParser
    ) => (path, node) => aParser(path, node).flatMapRight(a => bParser(path, a));

    [PublicAPI] public static Config.Parser<From, B> flatMap<From, A, B>(
      this Config.Parser<From, A> aParser, Func<ConfigPath, A, Either<ConfigLookupError, B>> f
    ) =>
      (path, o) => aParser(path, o).flatMapRight(a => f(path, a));

    [PublicAPI] public static Config.Parser<From, B> flatMapTry<From, A, B>(
      this Config.Parser<From, A> aParser, Func<ConfigPath, A, B> f
    ) =>
      (path, o) => aParser(path, o).flatMapRight(a => {
        try { return new Either<ConfigLookupError, B>(f(path, a)); }
        catch (ConfigFetchException e) { return new Either<ConfigLookupError, B>(e.error); }
        catch (Exception e) { return new Either<ConfigLookupError, B>(ConfigLookupError.fromException(e)); }
      });

    [PublicAPI] public static Config.Parser<From, A> filter<From, A>(
      this Config.Parser<From, A> parser, Func<A, bool> predicate
    ) =>
      (path, o) => parser(path, o).flatMapRight(a =>
        predicate(a)
        ? new Either<ConfigLookupError, A>(a)
        : Config.parseErrorEFor<A>(path, a, "didn't pass predicate")
      );

    [PublicAPI] public static Config.Parser<From, B> collect<From, A, B>(
      this Config.Parser<From, A> parser, Func<A, Option<B>> collector
    ) =>
      (path, o) => parser(path, o).flatMapRight(a => {
        var bOpt = collector(a);
        return bOpt.isSome
          ? new Either<ConfigLookupError, B>(bOpt.get)
          : Config.parseErrorEFor<B>(path, a, "didn't pass collector");
      });

    [PublicAPI] public static Config.Parser<From, A> or<From, A>(
      this Config.Parser<From, A> a1, Config.Parser<From, A> a2
    ) =>
      (path, node) => {
        var a1E = a1(path, node);
        return a1E.isRight ? a1E : a2(path, node);
      };

    [PublicAPI] public static Config.Parser<From, Tpl<A1, A2>> and<From, A1, A2>(
      this Config.Parser<From, A1> a1p, Config.Parser<From, A2> a2p
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

    [PublicAPI] public static Config.Parser<From, Tpl<A1, A2, A3>> and<From, A1, A2, A3>(
      this Config.Parser<From, A1> a1p, Config.Parser<From, A2> a2p, Config.Parser<From, A3> a3p
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

    [PublicAPI] public static Config.Parser<From, Tpl<A1, A2, A3, A4>> and<From, A1, A2, A3, A4>(
      this Config.Parser<From, A1> a1p, Config.Parser<From, A2> a2p, Config.Parser<From, A3> a3p, 
      Config.Parser<From, A4> a4p
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
