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
   * 
   * Beware that *String methods will always return a value, even if you expect
   * types to not match. This is because the underlying JSON library treats all
   * types as strings internally.
   **/
  public interface IConfig {
    /* scope of the config, "" if root, "foo.bar.baz" if nested. */
    string scope { get; }

    /* value if found, ArgumentException if not found. */

    #region getters

    string getString(string key);
    IList<string> getStringList(string key);
    int getInt(string key);
    IList<int> getIntList(string key);
    float getFloat(string key);
    IList<float> getFloatList(string key);
    bool getBool(string key);
    IList<bool> getBoolList(string key);
    IConfig getSubConfig(string key);
    IList<IConfig> getSubConfigList(string key);

    #endregion

    /* Some(value) if found, None if not found. */

    #region opt getters

    Option<string> optString(string key);
    Option<IList<string>> optStringList(string key);
    Option<int> optInt(string key);
    Option<IList<int>> optIntList(string key);
    Option<float> optFloat(string key);
    Option<IList<float>> optFloatList(string key);
    Option<bool> optBool(string key);
    Option<IList<bool>> optBoolList(string key);
    Option<IConfig> optSubConfig(string key);
    Option<IList<IConfig>> optSubConfigList(string key);

    #endregion

    /* Left(error message) on error, Right(value) if found. */

    #region either getters

    Either<string, string> eitherString(string key);
    Either<string, IList<string>> eitherStringList(string key);
    Either<string, int> eitherInt(string key);
    Either<string, IList<int>> eitherIntList(string key);
    Either<string, float> eitherFloat(string key);
    Either<string, IList<float>> eitherFloatList(string key);
    Either<string, bool> eitherBool(string key);
    Either<string, IList<bool>> eitherBoolList(string key);
    Either<string, IConfig> eitherSubConfig(string key);
    Either<string, IList<IConfig>> eitherSubConfigList(string key);

    #endregion

    /* Success(value) if found, Error(ArgumentException) if not found. */

    #region try getters

    Try<string> tryString(string key);
    Try<IList<string>> tryStringList(string key);
    Try<int> tryInt(string key);
    Try<IList<int>> tryIntList(string key);
    Try<float> tryFloat(string key);
    Try<IList<float>> tryFloatList(string key);
    Try<bool> tryBool(string key);
    Try<IList<bool>> tryBoolList(string key);
    Try<IConfig> trySubConfig(string key);
    Try<IList<IConfig>> trySubConfigList(string key);

    #endregion
  }
}
