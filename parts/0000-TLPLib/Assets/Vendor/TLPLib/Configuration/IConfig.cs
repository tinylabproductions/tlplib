using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using static com.tinylabproductions.TLPLib.Configuration.Config;

namespace com.tinylabproductions.TLPLib.Configuration {
  /**
   * Config class that fetches JSON configuration from `url`. Contents of
   * `url` are expected to be a JSON object.
   *
   * Create one with `Config.apply(url)` or `new Config(json)`.
   *
   * Paths are specified in "key.subkey.subsubkey" format.
   *
   * You can specify references by giving value in format of '#REF=some.config.key#'.
   **/
  public interface IConfig {
    /* scope of the config, "" if root, "foo.bar.baz" if nested. */
    ConfigPath scope { get; }
    /** Immediate keys of this config object. */
    ICollection<string> keys { get; }

    Either<ConfigLookupError, A> eitherGet<A>(string key, Parser<A> parser);
  }

  public static class IConfigExts {
    /* value if found, ConfigFetchException if not found. */

    #region getters

    public static A get<A>(this IConfig cfg, string key, Parser<A> parser) => 
      cfg.tryGet(key, parser).getOrThrow;
    public static List<A> getList<A>(this IConfig cfg, string key, Parser<A> parser) => 
      cfg.tryList(key, parser).getOrThrow;
    public static Dictionary<K, V> getDict<K, V>(
      this IConfig cfg, string key, Parser<K> keyParser, Parser<V> valueParser
    ) => cfg.tryDict(key, keyParser, valueParser).getOrThrow;

    public static object getObject(this IConfig cfg, string key) => cfg.get(key, objectParser);
    public static string getString(this IConfig cfg, string key) => cfg.get(key, stringParser);
    public static int getInt(this IConfig cfg, string key) => cfg.get(key, intParser);
    public static uint getUInt(this IConfig cfg, string key) => cfg.get(key, uintParser);
    public static long getLong(this IConfig cfg, string key) => cfg.get(key, longParser);
    public static ulong getULong(this IConfig cfg, string key) => cfg.get(key, ulongParser);
    public static float getFloat(this IConfig cfg, string key) => cfg.get(key, floatParser);
    public static double getDouble(this IConfig cfg, string key) => cfg.get(key, doubleParser);
    public static bool getBool(this IConfig cfg, string key) => cfg.get(key, boolParser);
    public static FRange getFRange(this IConfig cfg, string key) => cfg.get(key, fRangeParser);
    public static DateTime getDateTime(this IConfig cfg, string key) => cfg.get(key, dateTimeParser);
    public static IConfig getSubConfig(this IConfig cfg, string key) => cfg.get(key, configParser);
    public static IList<IConfig> getSubConfigList(this IConfig cfg, string key) => cfg.getList(key, configParser);

    #endregion

    /* Some(value) if found, None if not found or wrong type. */

    #region opt getters

    public static Option<A> optGet<A>(this IConfig cfg, string key, Parser<A> parser) =>
      cfg.eitherGet(key, parser).rightValue;

    public static Option<List<A>> optList<A>(this IConfig cfg, string key, Parser<A> parser) =>
      cfg.eitherList(key, parser).rightValue;

    public static Option<Dictionary<K, V>> optDict<K, V>(
      this IConfig cfg, string key, Parser<K> keyParser, Parser<V> valueParser
    ) => cfg.eitherDict(key, keyParser, valueParser).rightValue;

    public static Option<object> optObject(this IConfig cfg, string key) => cfg.optGet(key, objectParser);
    public static Option<string> optString(this IConfig cfg, string key) => cfg.optGet(key, stringParser);
    public static Option<int> optInt(this IConfig cfg, string key) => cfg.optGet(key, intParser);
    public static Option<uint> optUInt(this IConfig cfg, string key) => cfg.optGet(key, uintParser);
    public static Option<long> optLong(this IConfig cfg, string key) => cfg.optGet(key, longParser);
    public static Option<ulong> optULong(this IConfig cfg, string key) => cfg.optGet(key, ulongParser);
    public static Option<float> optFloat(this IConfig cfg, string key) => cfg.optGet(key, floatParser);
    public static Option<double> optDouble(this IConfig cfg, string key) => cfg.optGet(key, doubleParser);
    public static Option<bool> optBool(this IConfig cfg, string key) => cfg.optGet(key, boolParser);
    public static Option<FRange> optFRange(this IConfig cfg, string key) => cfg.optGet(key, fRangeParser);
    public static Option<DateTime> optDateTime(this IConfig cfg, string key) => cfg.optGet(key, dateTimeParser);
    public static Option<IConfig> optSubConfig(this IConfig cfg, string key) => cfg.optGet(key, configParser);
    public static Option<List<IConfig>> optSubConfigList(this IConfig cfg, string key) => cfg.optList(key, configParser);

    #endregion

    #region either getters

    public static Either<ConfigLookupError, List<A>> eitherList<A>(
      this IConfig cfg, string key, Parser<A> parser
    ) => cfg.eitherGet(key, listParser(parser));

    public static Either<ConfigLookupError, Dictionary<K, V>> eitherDict<K, V>(
      this IConfig baseCfg, string key, Parser<K> keyParser, Parser<V> valueParser
    ) => baseCfg.eitherGet(key, dictParser(keyParser, valueParser));

    public static Either<ConfigLookupError, object> eitherObject(this IConfig cfg, string key) => cfg.eitherGet(key, objectParser);
    public static Either<ConfigLookupError, string> eitherString(this IConfig cfg, string key) => cfg.eitherGet(key, stringParser);
    public static Either<ConfigLookupError, int> eitherInt(this IConfig cfg, string key) => cfg.eitherGet(key, intParser);
    public static Either<ConfigLookupError, uint> eitherUInt(this IConfig cfg, string key) => cfg.eitherGet(key, uintParser);
    public static Either<ConfigLookupError, long> eitherLong(this IConfig cfg, string key) => cfg.eitherGet(key, longParser);
    public static Either<ConfigLookupError, ulong> eitherULong(this IConfig cfg, string key) => cfg.eitherGet(key, ulongParser);
    public static Either<ConfigLookupError, float> eitherFloat(this IConfig cfg, string key) => cfg.eitherGet(key, floatParser);
    public static Either<ConfigLookupError, double> eitherDouble(this IConfig cfg, string key) => cfg.eitherGet(key, doubleParser);
    public static Either<ConfigLookupError, bool> eitherBool(this IConfig cfg, string key) => cfg.eitherGet(key, boolParser);
    public static Either<ConfigLookupError, FRange> eitherFRange(this IConfig cfg, string key) => cfg.eitherGet(key, fRangeParser);
    public static Either<ConfigLookupError, DateTime> eitherDateTime(this IConfig cfg, string key) => cfg.eitherGet(key, dateTimeParser);
    public static Either<ConfigLookupError, IConfig> eitherSubConfig(this IConfig cfg, string key) => cfg.eitherGet(key, configParser);
    public static Either<ConfigLookupError, List<IConfig>> eitherSubConfigList(this IConfig cfg, string key) => cfg.eitherList(key, configParser);

    #endregion

    /* Success(value) if found, Error(ConfigFetchException) if not found. */

    #region try getters

    public static Try<A> tryGet<A>(this IConfig cfg, string key, Parser<A> parser) => 
      e2t(cfg.eitherGet(key, parser));
    public static Try<List<A>> tryList<A>(this IConfig cfg, string key, Parser<A> parser) =>
      e2t(cfg.eitherList(key, parser));
    public static Try<Dictionary<K, V>> tryDict<K, V>(
      this IConfig cfg, string key, Parser<K> keyParser, Parser<V> valueParser
    ) => e2t(cfg.eitherDict(key, keyParser, valueParser));

    static Try<A> e2t<A>(Either<ConfigLookupError, A> e) =>
      e.isLeft
      ? F.err<A>(new ConfigFetchException(e.__unsafeGetLeft))
      : F.scs(e.__unsafeGetRight);

    public static Try<object> tryObject(this IConfig cfg, string key) => cfg.tryGet(key, objectParser);
    public static Try<string> tryString(this IConfig cfg, string key) => cfg.tryGet(key, stringParser);
    public static Try<int> tryInt(this IConfig cfg, string key) => cfg.tryGet(key, intParser);
    public static Try<uint> tryUInt(this IConfig cfg, string key) => cfg.tryGet(key, uintParser);
    public static Try<long> tryLong(this IConfig cfg, string key) => cfg.tryGet(key, longParser);
    public static Try<ulong> tryULong(this IConfig cfg, string key) => cfg.tryGet(key, ulongParser);
    public static Try<float> tryFloat(this IConfig cfg, string key) => cfg.tryGet(key, floatParser);
    public static Try<double> tryDouble(this IConfig cfg, string key) => cfg.tryGet(key, doubleParser);
    public static Try<bool> tryBool(this IConfig cfg, string key) => cfg.tryGet(key, boolParser);
    public static Try<FRange> tryFRange(this IConfig cfg, string key) => cfg.tryGet(key, fRangeParser);
    public static Try<DateTime> tryDateTime(this IConfig cfg, string key) => cfg.tryGet(key, dateTimeParser);
    public static Try<IConfig> trySubConfig(this IConfig cfg, string key) => cfg.tryGet(key, configParser);
    public static Try<List<IConfig>> trySubConfigList(this IConfig cfg, string key) => cfg.tryList(key, configParser);

    #endregion
  }

  public struct ConfigLookupError {
    public enum Kind { KEY_NOT_FOUND, WRONG_TYPE }

    public readonly Kind kind;
    public readonly Lazy<string> messageLazy;
    public string message => messageLazy.get;

    public ConfigLookupError(Kind kind, Lazy<string> messageLazy) {
      this.kind = kind;
      this.messageLazy = messageLazy;
    }

    public static ConfigLookupError keyNotFound(Lazy<string> message) => 
      new ConfigLookupError(Kind.KEY_NOT_FOUND, message);

    public static ConfigLookupError wrongType(Lazy<string> message) => 
      new ConfigLookupError(Kind.WRONG_TYPE, message);

    public override string ToString() => $"{nameof(ConfigLookupError)}[{kind}, {message}]";
  }

  public class ConfigFetchException : Exception {
    public readonly ConfigLookupError error;

    public ConfigFetchException(ConfigLookupError error) : base(error.ToString())
    { this.error = error; }
  }
}
