using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data {
  public class PrefValStorage {
    readonly IPrefValueBackend backend;

    public PrefValStorage(IPrefValueBackend backend) { this.backend = backend; }

    public bool hasKey(string name) => backend.hasKey(name);

    public PrefVal<A> create<A>(
      string key, A defaultVal, IPrefValueRW<A> rw, bool saveOnEveryWrite = false
    ) => new PrefValImpl<A>(key, rw, defaultVal, backend, saveOnEveryWrite);

    public PrefVal<string> str(string key, string defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.str, saveOnEveryWrite);

    public PrefVal<Uri> uri(string key, Uri defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.uri, saveOnEveryWrite);

    public PrefVal<int> integer(string key, int defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.integer, saveOnEveryWrite);

    public PrefVal<uint> uinteger(string key, uint defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.uinteger, saveOnEveryWrite);

    public PrefVal<float> flt(string key, float defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.flt, saveOnEveryWrite);

    public PrefVal<bool> boolean(string key, bool defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.boolean, saveOnEveryWrite);

    public PrefVal<Duration> duration(string key, Duration defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.duration, saveOnEveryWrite);

    public PrefVal<DateTime> dateTime(string key, DateTime defaultVal, bool saveOnEveryWrite = false) =>
      create(key, defaultVal, PrefValRW.dateTime, saveOnEveryWrite);

    #region Collections

    [PublicAPI]
    public PrefVal<ImmutableArray<A>> array<A>(
      string key, ISerializedRW<A> rw,
      ImmutableArray<A> defaultVal, bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => collection(
      key, rw, CollectionBuilderKnownSizeFactory<A>.immutableArray, defaultVal, saveOnEveryWrite,
      onDeserializeFailure, log
    );

    [PublicAPI]
    public PrefVal<ImmutableList<A>> list<A>(
      string key, ISerializedRW<A> rw,
      ImmutableList<A> defaultVal = null, bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => collection(
      key, rw, CollectionBuilderKnownSizeFactory<A>.immutableList, 
      defaultVal ?? ImmutableList<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, log
    );

    [PublicAPI]
    public PrefVal<ImmutableHashSet<A>> hashSet<A>(
      string key, ISerializedRW<A> rw,
      ImmutableHashSet<A> defaultVal = null, bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => collection(
      key, rw, CollectionBuilderKnownSizeFactory<A>.immutableHashSet, 
      defaultVal ?? ImmutableHashSet<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, log
    );

    #endregion

    #region Custom

    /* Provide custom mapping. It uses string representation inside and returns
     * default value if string is empty. */
    [Obsolete]
    public PrefVal<A> custom__OLD<A>(
      string key, A defaultVal, Fn<A, string> map, Fn<string, A> comap, bool saveOnEveryWrite=true
    ) => create(key, defaultVal, PrefValRW.custom__OLD(map, comap), saveOnEveryWrite);

    public PrefVal<A> custom<A>(
      string key, A defaultVal,
      Fn<A, string> serialize, Fn<string, Option<A>> deserialize,
      bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(
      key, defaultVal, PrefValRW.custom(serialize, deserialize, onDeserializeFailure, log),
      saveOnEveryWrite
    );

    public PrefVal<A> custom<A>(
      string key, A defaultVal,
      ISerializedRW<A> aRW,
      bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(
      key, defaultVal, PrefValRW.custom(aRW, onDeserializeFailure, log), saveOnEveryWrite
    );

    public PrefVal<Option<A>> opt<A>(
      string key, Option<A> defaultVal,
      ISerializedRW<A> aRW,
      bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(key, defaultVal, PrefValRW.opt(aRW, onDeserializeFailure, log), saveOnEveryWrite);

    #endregion

    #region Custom Collection

    public PrefVal<C> collection<A, C>(
      string key,
      ISerializedRW<A> rw, CollectionBuilderKnownSizeFactory<A, C> factory,
      C defaultVal, bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) where C : ICollection<A> {
      var collectionRw = SerializedRW.a(
        SerializedRW.collectionSerializer<A, C>(rw),
        SerializedRW.collectionDeserializer(rw, factory)
      );
      return collection<A, C>(
        key, collectionRw, defaultVal, saveOnEveryWrite, onDeserializeFailure, log
      );
    }

    public PrefVal<C> collection<A, C>(
      string key, ISerializedRW<C> rw, C defaultVal, bool saveOnEveryWrite = false,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) where C : ICollection<A> =>
      custom(key, defaultVal, rw, saveOnEveryWrite, onDeserializeFailure, log);

    #endregion
  }
}