using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Configuration {
  /* Base configuration that any class extending IConfig should implement. */
  public abstract class ConfigBase : IConfig {
    public abstract string scope { get; }

    #region getters

    public object getObject(string key)
    { return tryObject(key).getOrThrow; }

    public A get<A>(string key, Config.Parser<A> parser) => 
      tryGet(key, parser).getOrThrow;

    public string getString(string key)
    { return tryString(key).getOrThrow; }

    public int getInt(string key)
    { return tryInt(key).getOrThrow; }

    public uint getUInt(string key)
    { return tryUInt(key).getOrThrow; }

    public long getLong(string key)
    { return tryLong(key).getOrThrow; }

    public ulong getULong(string key)
    { return tryULong(key).getOrThrow; }

    public float getFloat(string key)
    { return tryFloat(key).getOrThrow; }

    public double getDouble(string key)
    { return tryDouble(key).getOrThrow; }

    public bool getBool(string key)
    { return tryBool(key).getOrThrow; }

    public FRange getFRange(string key)
    { return tryFRange(key).getOrThrow; }

    public DateTime getDateTime(string key)
    { return tryDateTime(key).getOrThrow; }

    public IConfig getSubConfig(string key)
    { return trySubConfig(key).getOrThrow; }

    public IList<IConfig> getSubConfigList(string key)
    { return trySubConfigList(key).getOrThrow; }

    public IList<A> getList<A>(string key, Config.Parser<A> parser)
    { return tryList(key, parser).getOrThrow; }

    #endregion

    #region opt getters

    public Option<object> optObject(string key)
    { return eitherObject(key).toOpt(); }

    public Option<A> optGet<A>(string key, Config.Parser<A> parser) => 
      eitherGet(key, parser).toOpt();

    public Option<string> optString(string key)
    { return eitherString(key).toOpt(); }

    public Option<int> optInt(string key)
    { return eitherInt(key).toOpt(); }

    public Option<uint> optUInt(string key)
    { return eitherUInt(key).toOpt(); }

    public Option<long> optLong(string key)
    { return eitherLong(key).toOpt(); }

    public Option<ulong> optULong(string key)
    { return eitherULong(key).toOpt(); }

    public Option<float> optFloat(string key)
    { return eitherFloat(key).toOpt(); }

    public Option<double> optDouble(string key)
    { return eitherDouble(key).toOpt(); }

    public Option<bool> optBool(string key)
    { return eitherBool(key).toOpt(); }

    public Option<FRange> optFRange(string key)
    { return eitherFRange(key).toOpt(); }

    public Option<DateTime> optDateTime(string key)
    { return eitherDateTime(key).toOpt(); }

    public Option<IConfig> optSubConfig(string key)
    { return eitherSubConfig(key).toOpt(); }

    public Option<IList<IConfig>> optSubConfigList(string key)
    { return eitherSubConfigList(key).toOpt(); }

    public Option<IList<A>> optList<A>(string key, Config.Parser<A> parser)
    { return eitherList(key, parser).toOpt(); }

    #endregion

    #region try getters

    public Try<object> tryObject(string key) { return eitherObject(key).fold(tryEx<object>, F.scs); }
    public Try<A> tryGet<A>(string key, Config.Parser<A> parser) => eitherGet(key, parser).fold(tryEx<A>, F.scs);
    public Try<string> tryString(string key) { return eitherString(key).fold(tryEx<string>, F.scs); }
    public Try<int> tryInt(string key) { return eitherInt(key).fold(tryEx<int>, F.scs); }
    public Try<uint> tryUInt(string key) { return eitherUInt(key).fold(tryEx<uint>, F.scs); }
    public Try<long> tryLong(string key) { return eitherLong(key).fold(tryEx<long>, F.scs); }
    public Try<ulong> tryULong(string key) { return eitherULong(key).fold(tryEx<ulong>, F.scs); }
    public Try<float> tryFloat(string key) { return eitherFloat(key).fold(tryEx<float>, F.scs); }
    public Try<double> tryDouble(string key) { return eitherDouble(key).fold(tryEx<double>, F.scs); }
    public Try<bool> tryBool(string key) { return eitherBool(key).fold(tryEx<bool>, F.scs); }
    public Try<FRange> tryFRange(string key) { return eitherFRange(key).fold(tryEx<FRange>, F.scs); }
    public Try<DateTime> tryDateTime(string key) { return eitherDateTime(key).fold(tryEx<DateTime>, F.scs); }
    public Try<IConfig> trySubConfig(string key) { return eitherSubConfig(key).fold(tryEx<IConfig>, F.scs); }
    public Try<IList<IConfig>> trySubConfigList(string key) { return eitherSubConfigList(key).fold(tryEx<IList<IConfig>>, F.scs); }

    public Try<IList<A>> tryList<A>(string key, Config.Parser<A> parser)
    { return eitherList(key, parser).fold(tryEx<IList<A>>, F.scs); }

    static Try<A> tryEx<A>(ConfigFetchError error) {
      return F.err<A>(new ConfigFetchException(error));
    }

    #endregion

    #region either getters

    public abstract Either<ConfigFetchError, object> eitherObject(string key);
    public abstract Either<ConfigFetchError, A> eitherGet<A>(string key, Config.Parser<A> parser);
    public abstract Either<ConfigFetchError, string> eitherString(string key);
    public abstract Either<ConfigFetchError, int> eitherInt(string key);
    public abstract Either<ConfigFetchError, uint> eitherUInt(string key);
    public abstract Either<ConfigFetchError, long> eitherLong(string key);
    public abstract Either<ConfigFetchError, ulong> eitherULong(string key);
    public abstract Either<ConfigFetchError, float> eitherFloat(string key);
    public abstract Either<ConfigFetchError, double> eitherDouble(string key);
    public abstract Either<ConfigFetchError, bool> eitherBool(string key);
    public abstract Either<ConfigFetchError, FRange> eitherFRange(string key);
    public abstract Either<ConfigFetchError, DateTime> eitherDateTime(string key);
    public abstract Either<ConfigFetchError, IConfig> eitherSubConfig(string key);
    public abstract Either<ConfigFetchError, IList<IConfig>> eitherSubConfigList(string key);
    public abstract Either<ConfigFetchError, IList<A>> eitherList<A>(string key, Config.Parser<A> parser);

    #endregion
  }
}
