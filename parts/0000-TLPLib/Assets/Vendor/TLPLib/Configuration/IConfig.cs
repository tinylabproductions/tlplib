using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;

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
    string scope { get; }

    /* value if found, ConfigFetchException if not found. */

    #region getters

    object getObject(string key);
    A get<A>(string key, Config.Parser<A> parser);
    string getString(string key);
    int getInt(string key);
    uint getUInt(string key);
    long getLong(string key);
    ulong getULong(string key);
    float getFloat(string key);
    double getDouble(string key);
    bool getBool(string key);
    FRange getFRange(string key);
    DateTime getDateTime(string key);
    IConfig getSubConfig(string key);
    IList<IConfig> getSubConfigList(string key);
    IList<A> getList<A>(string key, Config.Parser<A> parser);

    #endregion

    /* Some(value) if found, None if not found or wrong type. */

    #region opt getters

    Option<object> optObject(string key);
    Option<A> optGet<A>(string key, Config.Parser<A> parser);
    Option<string> optString(string key);
    Option<int> optInt(string key);
    Option<uint> optUInt(string key);
    Option<long> optLong(string key);
    Option<ulong> optULong(string key);
    Option<float> optFloat(string key);
    Option<double> optDouble(string key);
    Option<bool> optBool(string key);
    Option<FRange> optFRange(string key);
    Option<DateTime> optDateTime(string key);
    Option<IConfig> optSubConfig(string key);
    Option<IList<IConfig>> optSubConfigList(string key);
    Option<IList<A>> optList<A>(string key, Config.Parser<A> parser);

    #endregion

    /* Left(ConfigFetchError) on error, Right(value) if found. */

    #region either getters

    Either<ConfigFetchError, object> eitherObject(string key);
    Either<ConfigFetchError, A> eitherGet<A>(string key, Config.Parser<A> parser);
    Either<ConfigFetchError, string> eitherString(string key);
    Either<ConfigFetchError, int> eitherInt(string key);
    Either<ConfigFetchError, uint> eitherUInt(string key);
    Either<ConfigFetchError, long> eitherLong(string key);
    Either<ConfigFetchError, ulong> eitherULong(string key);
    Either<ConfigFetchError, float> eitherFloat(string key);
    Either<ConfigFetchError, double> eitherDouble(string key);
    Either<ConfigFetchError, bool> eitherBool(string key);
    Either<ConfigFetchError, FRange> eitherFRange(string key);
    Either<ConfigFetchError, DateTime> eitherDateTime(string key);
    Either<ConfigFetchError, IConfig> eitherSubConfig(string key);
    Either<ConfigFetchError, IList<IConfig>> eitherSubConfigList(string key);
    Either<ConfigFetchError, IList<A>> eitherList<A>(string key, Config.Parser<A> parser);

    #endregion

    /* Success(value) if found, Error(ConfigFetchException) if not found. */

    #region try getters

    Try<object> tryObject(string key);
    Try<A> tryGet<A>(string key, Config.Parser<A> parser);
    Try<string> tryString(string key);
    Try<int> tryInt(string key);
    Try<uint> tryUInt(string key);
    Try<long> tryLong(string key);
    Try<ulong> tryULong(string key);
    Try<float> tryFloat(string key);
    Try<double> tryDouble(string key);
    Try<bool> tryBool(string key);
    Try<FRange> tryFRange(string key);
    Try<DateTime> tryDateTime(string key);
    Try<IConfig> trySubConfig(string key);
    Try<IList<IConfig>> trySubConfigList(string key);
    Try<IList<A>> tryList<A>(string key, Config.Parser<A> parser);

    #endregion
  }

  public struct ConfigFetchError {
    public enum Kind { KEY_NOT_FOUND, WRONG_TYPE, BROKEN_REFERENCE }

    public readonly Kind kind;
    public readonly string message;

    public ConfigFetchError(Kind kind, string message) {
      this.kind = kind;
      this.message = message;
    }

    public static ConfigFetchError keyNotFound(string message)
    { return new ConfigFetchError(Kind.KEY_NOT_FOUND, message); }

    public static ConfigFetchError wrongType(string message)
    { return new ConfigFetchError(Kind.WRONG_TYPE, message); }

    public static ConfigFetchError brokenRef(string message)
    { return new ConfigFetchError(Kind.BROKEN_REFERENCE, message); }

    public override string ToString() {
      return $"ConfigFetchError[kind: {kind}, message: {message}]";
    }
  }

  public class ConfigFetchException : Exception {
    public readonly ConfigFetchError error;

    public ConfigFetchException(ConfigFetchError error) : base(error.ToString())
    { this.error = error; }
  }
}
