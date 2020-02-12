using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.serialization;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Data {
  public class PrefValStorage {
    public readonly IPrefValueBackend backend;

    public PrefValStorage(IPrefValueBackend backend) { this.backend = backend; }

    public bool hasKey(string name) => backend.hasKey(name);

    public PrefVal<A> create<A>(
      string key, A defaultVal, IPrefValueRW<A> rw
    ) => new PrefValImpl<A>(key, rw, defaultVal, backend);

    public PrefVal<string> str(string key, string defaultVal) =>
      create(key, defaultVal, PrefValRW.str);

    public PrefVal<Uri> uri(string key, Uri defaultVal) =>
      create(key, defaultVal, PrefValRW.uri);

    public PrefVal<int> integer(string key, int defaultVal) =>
      create(key, defaultVal, PrefValRW.integer);

    public PrefVal<uint> uinteger(string key, uint defaultVal) =>
      create(key, defaultVal, PrefValRW.uinteger);

    public PrefVal<float> flt(string key, float defaultVal) =>
      create(key, defaultVal, PrefValRW.flt);

    public PrefVal<bool> boolean(string key, bool defaultVal) =>
      create(key, defaultVal, PrefValRW.boolean);

    public PrefVal<Duration> duration(string key, Duration defaultVal) =>
      create(key, defaultVal, PrefValRW.duration);

    public PrefVal<DateTime> dateTime(string key, DateTime defaultVal) =>
      create(key, defaultVal, PrefValRW.dateTime);

    #region Collections

    [PublicAPI]
    public PrefVal<ImmutableArray<A>> array<A>(
      string key, ISerializedRW<A> rw,
      ImmutableArray<A> defaultVal,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => collection(
      key, rw, CollectionBuilderKnownSizeFactory<A>.immutableArray, defaultVal,
      onDeserializeFailure, log
    );

    [PublicAPI]
    public PrefVal<ImmutableList<A>> list<A>(
      string key, ISerializedRW<A> rw,
      ImmutableList<A> defaultVal = null,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => collection(
      key, rw, CollectionBuilderKnownSizeFactory<A>.immutableList, 
      defaultVal ?? ImmutableList<A>.Empty,
      onDeserializeFailure, log
    );

    [PublicAPI]
    public PrefVal<ImmutableHashSet<A>> hashSet<A>(
      string key, ISerializedRW<A> rw,
      ImmutableHashSet<A> defaultVal = null,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => collection(
      key, rw, CollectionBuilderKnownSizeFactory<A>.immutableHashSet, 
      defaultVal ?? ImmutableHashSet<A>.Empty,
      onDeserializeFailure, log
    );
    
    [PublicAPI]
    public PrefValDictionary<K, V> dictionary<K, V>(
      string key, Func<K, string> keyToString, ISerializedRW<V> vRw, V defaultValue,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => new PrefValDictImpl<K, V>(key, keyToString, vRw, this, defaultValue, onDeserializeFailure, log);

    #endregion

    #region Custom

    public PrefVal<A> custom<A>(
      string key, A defaultVal,
      Func<A, string> serialize, Func<string, Either<string, A>> deserialize,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(
      key, defaultVal, PrefValRW.custom(serialize, deserialize, onDeserializeFailure, log)
    );

    public PrefVal<A> custom<A>(
      string key, A defaultVal,
      ISerializedRW<A> aRW,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(
      key, defaultVal, PrefValRW.custom(aRW, onDeserializeFailure, log)
    );

    public PrefVal<Option<A>> opt<A>(
      string key, Option<A> defaultVal,
      ISerializedRW<A> aRW,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(key, defaultVal, PrefValRW.opt(aRW, onDeserializeFailure, log));

    #endregion

    #region Custom Collection

    public PrefVal<C> collection<A, C>(
      string key,
      ISerializedRW<A> rw, CollectionBuilderKnownSizeFactory<A, C> factory,
      C defaultVal,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) where C : IReadOnlyCollection<A> {
      var collectionRw = SerializedRW.a(
        SerializedRW.collectionSerializer<A, C>(rw),
        SerializedRW.collectionDeserializer(rw, factory)
      );
      return collection<A, C>(
        key, collectionRw, defaultVal, onDeserializeFailure, log
      );
    }

    public PrefVal<C> collection<A, C>(
      string key, ISerializedRW<C> rw, C defaultVal,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) where C : IReadOnlyCollection<A> =>
      custom(key, defaultVal, rw, onDeserializeFailure, log);

    #endregion
  }
}