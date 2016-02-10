using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Configuration {
  /* Base configuration that any class extending IConfig should implement. */
  public abstract class ConfigBase : IConfig {
    public abstract string scope { get; }

    #region getters

    public string getString(string key) 
    { return tryString(key).getOrThrow; }

    public IList<string> getStringList(string key) 
    { return tryStringList(key).getOrThrow; }

    public int getInt(string key) 
    { return tryInt(key).getOrThrow; }

    public IList<int> getIntList(string key) 
    { return tryIntList(key).getOrThrow; }

    public long getLong(string key)
    { return tryLong(key).getOrThrow; }

    public IList<long> getLongList(string key)
    { return tryLongList(key).getOrThrow; }

    public float getFloat(string key) 
    { return tryFloat(key).getOrThrow; }

    public IList<float> getFloatList(string key) 
    { return tryFloatList(key).getOrThrow; }

    public bool getBool(string key) 
    { return tryBool(key).getOrThrow; }

    public IList<bool> getBoolList(string key) 
    { return tryBoolList(key).getOrThrow; }

    public DateTime getDateTime(string key) 
    { return tryDateTime(key).getOrThrow; }

    public IList<DateTime> getDateTimeList(string key) 
    { return tryDateTimeList(key).getOrThrow; }

    public IConfig getSubConfig(string key) 
    { return trySubConfig(key).getOrThrow; }
    public IList<IConfig> getSubConfigList(string key) 
    { return trySubConfigList(key).getOrThrow; }

    #endregion

    #region opt getters

    public Option<string> optString(string key) 
    { return eitherString(key).toOpt(); }

    public Option<IList<string>> optStringList(string key) 
    { return eitherStringList(key).toOpt(); }

    public Option<int> optInt(string key) 
    { return eitherInt(key).toOpt(); }

    public Option<IList<int>> optIntList(string key) 
    { return eitherIntList(key).toOpt(); }

    public Option<long> optLong(string key)
    { return eitherLong(key).toOpt(); }

    public Option<IList<long>> optLongList(string key)
    { return eitherLongList(key).toOpt(); }

    public Option<float> optFloat(string key) 
    { return eitherFloat(key).toOpt(); }

    public Option<IList<float>> optFloatList(string key) 
    { return eitherFloatList(key).toOpt(); }

    public Option<bool> optBool(string key) 
    { return eitherBool(key).toOpt(); }

    public Option<IList<bool>> optBoolList(string key) 
    { return eitherBoolList(key).toOpt(); }

    public Option<DateTime> optDateTime(string key) 
    { return eitherDateTime(key).toOpt(); }

    public Option<IList<DateTime>> optDateTimeList(string key) 
    { return eitherDateTimeList(key).toOpt(); }

    public Option<IConfig> optSubConfig(string key) 
    { return eitherSubConfig(key).toOpt(); }

    public Option<IList<IConfig>> optSubConfigList(string key) 
    { return eitherSubConfigList(key).toOpt(); }

    #endregion

    #region try getters

    public Try<string> tryString(string key) {
      return eitherString(key).fold(tryEx<string>, F.scs);
    }

    public Try<IList<string>> tryStringList(string key) {
      return eitherStringList(key).fold(tryEx<IList<string>>, F.scs);
    }

    public Try<int> tryInt(string key) {
      return eitherInt(key).fold(tryEx<int>, F.scs);
    }

    public Try<IList<int>> tryIntList(string key) {
      return eitherIntList(key).fold(tryEx<IList<int>>, F.scs);
    }

    public Try<long> tryLong(string key) { return eitherLong(key).fold(tryEx<long>, F.scs); }

    public Try<IList<long>> tryLongList(string key) { return eitherLongList(key).fold(tryEx<IList<long>>, F.scs); }

    public Try<float> tryFloat(string key) {
      return eitherFloat(key).fold(tryEx<float>, F.scs);
    }

    public Try<IList<float>> tryFloatList(string key) {
      return eitherFloatList(key).fold(tryEx<IList<float>>, F.scs);
    }

    public Try<bool> tryBool(string key) {
      return eitherBool(key).fold(tryEx<bool>, F.scs);
    }

    public Try<IList<bool>> tryBoolList(string key) {
      return eitherBoolList(key).fold(tryEx<IList<bool>>, F.scs);
    }

    public Try<DateTime> tryDateTime(string key) {
      return eitherDateTime(key).fold(tryEx<DateTime>, F.scs);
    }

    public Try<IList<DateTime>> tryDateTimeList(string key) {
      return eitherDateTimeList(key).fold(tryEx<IList<DateTime>>, F.scs);
    }

    public Try<IConfig> trySubConfig(string key) {
      return eitherSubConfig(key).fold(tryEx<IConfig>, F.scs);
    }

    public Try<IList<IConfig>> trySubConfigList(string key) {
      return eitherSubConfigList(key).fold(tryEx<IList<IConfig>>, F.scs);
    }

    private static Try<A> tryEx<A>(ConfigFetchError error) {
      return F.err<A>(new ConfigFetchException(error));
    }

    #endregion

    #region either getters

    public abstract Either<ConfigFetchError, string> eitherString(string key);
    public abstract Either<ConfigFetchError, IList<string>> eitherStringList(string key);
    public abstract Either<ConfigFetchError, int> eitherInt(string key);
    public abstract Either<ConfigFetchError, IList<int>> eitherIntList(string key);
    public abstract Either<ConfigFetchError, long> eitherLong(string key);
    public abstract Either<ConfigFetchError, IList<long>> eitherLongList(string key);
    public abstract Either<ConfigFetchError, float> eitherFloat(string key);
    public abstract Either<ConfigFetchError, IList<float>> eitherFloatList(string key);
    public abstract Either<ConfigFetchError, bool> eitherBool(string key);
    public abstract Either<ConfigFetchError, IList<bool>> eitherBoolList(string key);
    public abstract Either<ConfigFetchError, DateTime> eitherDateTime(string key);
    public abstract Either<ConfigFetchError, IList<DateTime>> eitherDateTimeList(string key);
    public abstract Either<ConfigFetchError, IConfig> eitherSubConfig(string key);
    public abstract Either<ConfigFetchError, IList<IConfig>> eitherSubConfigList(string key);

    #endregion
  }
}
