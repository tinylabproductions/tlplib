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

    public Try<object> tryObject(string key) => e2t(eitherObject(key));
    public Try<A> tryGet<A>(string key, Config.Parser<A> parser) => e2t(eitherGet(key, parser));
    public Try<string> tryString(string key) => e2t(eitherString(key));
    public Try<int> tryInt(string key) => e2t(eitherInt(key));
    public Try<uint> tryUInt(string key) => e2t(eitherUInt(key));
    public Try<long> tryLong(string key) => e2t(eitherLong(key));
    public Try<ulong> tryULong(string key) => e2t(eitherULong(key));
    public Try<float> tryFloat(string key) => e2t(eitherFloat(key));
    public Try<double> tryDouble(string key) => e2t(eitherDouble(key));
    public Try<bool> tryBool(string key) => e2t(eitherBool(key));
    public Try<FRange> tryFRange(string key) => e2t(eitherFRange(key));
    public Try<DateTime> tryDateTime(string key) => e2t(eitherDateTime(key));
    public Try<IConfig> trySubConfig(string key) => e2t(eitherSubConfig(key));
    public Try<IList<IConfig>> trySubConfigList(string key) => e2t(eitherSubConfigList(key));
    public Try<IList<A>> tryList<A>(string key, Config.Parser<A> parser) => e2t(eitherList(key, parser));

    static Try<A> e2t<A>(Either<ConfigFetchError, A> e) => 
      e.isLeft 
      ? F.err<A>(new ConfigFetchException(e.__unsafeGetLeft)) 
      : F.scs(e.__unsafeGetRight);

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
