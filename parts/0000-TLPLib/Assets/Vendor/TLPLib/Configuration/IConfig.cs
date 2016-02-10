using System;
using System.Collections.Generic;
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

    string getString(string key);
    IList<string> getStringList(string key);
    int getInt(string key);
    IList<int> getIntList(string key);
    long getLong(string key);
    IList<long> getLongList(string key);
    float getFloat(string key);
    IList<float> getFloatList(string key);
    bool getBool(string key);
    IList<bool> getBoolList(string key);
    DateTime getDateTime(string key);
    IList<DateTime> getDateTimeList(string key);
    IConfig getSubConfig(string key);
    IList<IConfig> getSubConfigList(string key);

    #endregion

    /* Some(value) if found, None if not found. */

    #region opt getters

    Option<string> optString(string key);
    Option<IList<string>> optStringList(string key);
    Option<int> optInt(string key);
    Option<IList<int>> optIntList(string key);
    Option<long> optLong(string key);
    Option<IList<long>> optLongList(string key);
    Option<float> optFloat(string key);
    Option<IList<float>> optFloatList(string key);
    Option<bool> optBool(string key);
    Option<IList<bool>> optBoolList(string key);
    Option<DateTime> optDateTime(string key);
    Option<IList<DateTime>> optDateTimeList(string key);
    Option<IConfig> optSubConfig(string key);
    Option<IList<IConfig>> optSubConfigList(string key);

    #endregion

    /* Left(ConfigFetchError) on error, Right(value) if found. */

    #region either getters

    Either<ConfigFetchError, string> eitherString(string key);
    Either<ConfigFetchError, IList<string>> eitherStringList(string key);
    Either<ConfigFetchError, int> eitherInt(string key);
    Either<ConfigFetchError, IList<int>> eitherIntList(string key);
    Either<ConfigFetchError, long> eitherLong(string key);
    Either<ConfigFetchError, IList<long>> eitherLongList(string key);
    Either<ConfigFetchError, float> eitherFloat(string key);
    Either<ConfigFetchError, IList<float>> eitherFloatList(string key);
    Either<ConfigFetchError, bool> eitherBool(string key);
    Either<ConfigFetchError, IList<bool>> eitherBoolList(string key);
    Either<ConfigFetchError, DateTime> eitherDateTime(string key);
    Either<ConfigFetchError, IList<DateTime>> eitherDateTimeList(string key);
    Either<ConfigFetchError, IConfig> eitherSubConfig(string key);
    Either<ConfigFetchError, IList<IConfig>> eitherSubConfigList(string key);

    #endregion

    /* Success(value) if found, Error(ConfigFetchException) if not found. */

    #region try getters

    Try<string> tryString(string key);
    Try<IList<string>> tryStringList(string key);
    Try<int> tryInt(string key);
    Try<IList<int>> tryIntList(string key);
    Try<long> tryLong(string key);
    Try<IList<long>> tryLongList(string key);
    Try<float> tryFloat(string key);
    Try<IList<float>> tryFloatList(string key);
    Try<bool> tryBool(string key);
    Try<IList<bool>> tryBoolList(string key);
    Try<DateTime> tryDateTime(string key);
    Try<IList<DateTime>> tryDateTimeList(string key);
    Try<IConfig> trySubConfig(string key);
    Try<IList<IConfig>> trySubConfigList(string key);

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
