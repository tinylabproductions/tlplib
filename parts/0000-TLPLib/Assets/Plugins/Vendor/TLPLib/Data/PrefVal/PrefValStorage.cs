using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Data {
  public class PrefValStorage {
    readonly IPrefValueBackend backend;

    public PrefValStorage(IPrefValueBackend backend) { this.backend = backend; }

    public PrefVal<A> create<A>(
      string key, A defaultVal, IPrefValueRW<A> rw, bool saveOnEveryWrite = true
    ) => new PrefValImpl<A>(key, rw, defaultVal, backend, saveOnEveryWrite);

    public PrefVal<string> str(string key, string defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.str, saveOnEveryWrite);

    public PrefVal<int> integer(string key, int defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.integer, saveOnEveryWrite);

    public PrefVal<uint> uinteger(string key, uint defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.uinteger, saveOnEveryWrite);

    public PrefVal<float> flt(string key, float defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.flt, saveOnEveryWrite);
    
    public PrefVal<bool> boolean(string key, bool defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.boolean, saveOnEveryWrite);

    public PrefVal<Duration> duration(string key, Duration defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.duration, saveOnEveryWrite);
    
    public PrefVal<DateTime> dateTime(string key, DateTime defaultVal, bool saveOnEveryWrite = true) =>
      create(key, defaultVal, PrefValRW.dateTime, saveOnEveryWrite);
    
    #region Collections

    public PrefVal<ImmutableArray<A>> array<A>(
      string key, ISerializedRW<A> rw,
      ImmutableArray<A> defaultVal, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      SerializedRW.OnCollectionItemDeserializationFailure onDeserializeCollectionItemFailure =
        SerializedRW.OnCollectionItemDeserializationFailure.Ignore,
      ILog log = null
    ) => collection(
      key, rw, a => a, defaultVal, saveOnEveryWrite, 
      onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    public PrefVal<ImmutableList<A>> list<A>(
      string key, ISerializedRW<A> rw,
      ImmutableList<A> defaultVal = null, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      SerializedRW.OnCollectionItemDeserializationFailure onDeserializeCollectionItemFailure =
        SerializedRW.OnCollectionItemDeserializationFailure.Ignore,
      ILog log = null
    ) => collection(
      key, rw, a => a.ToImmutableList(), defaultVal ?? ImmutableList<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    public PrefVal<ImmutableHashSet<A>> hashSet<A>(
      string key, ISerializedRW<A> rw,
      ImmutableHashSet<A> defaultVal = null, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      SerializedRW.OnCollectionItemDeserializationFailure onDeserializeCollectionItemFailure =
        SerializedRW.OnCollectionItemDeserializationFailure.Ignore,
      ILog log = null
    ) => collection(
      key, rw, a => a.ToImmutableHashSet(), defaultVal ?? ImmutableHashSet<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
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
      bool saveOnEveryWrite = true, 
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(
      key, defaultVal, PrefValRW.custom(serialize, deserialize, onDeserializeFailure, log), 
      saveOnEveryWrite
    );

    public PrefVal<A> custom<A>(
      string key, A defaultVal, 
      ISerializedRW<A> baRW,
      bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => create(
      key, defaultVal, PrefValRW.custom(baRW, onDeserializeFailure, log), saveOnEveryWrite
    );

    #endregion
    
    #region Custom Collection

    public PrefVal<C> collection<A, C>(
      string key,
      ISerializedRW<A> rw, Fn<ImmutableArray<A>, C> toCollection,
      C defaultVal, bool saveOnEveryWrite = true, 
      PrefVal.OnDeserializeFailure onDeserializeFailure = 
        PrefVal.OnDeserializeFailure.ReturnDefault,
      SerializedRW.OnCollectionItemDeserializationFailure onDeserializeCollectionItemFailure =
        SerializedRW.OnCollectionItemDeserializationFailure.Ignore,
      ILog log = null
    ) where C : ICollection<A> {
      var collectionRw = SerializedRW.a(
        SerializedRW.collectionSerializer<A, C>(rw),
        SerializedRW
          .collectionDeserializer(rw, onDeserializeCollectionItemFailure)
          .map(arr => toCollection(arr).some())
      );
      return collection<A, C>(
        key, collectionRw, defaultVal, saveOnEveryWrite, onDeserializeFailure, log
      );
    }

    public PrefVal<C> collection<A, C>(
      string key, ISerializedRW<C> rw, C defaultVal, bool saveOnEveryWrite = true, 
      PrefVal.OnDeserializeFailure onDeserializeFailure = 
        PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) where C : ICollection<A> => 
      custom(key, defaultVal, rw, saveOnEveryWrite, onDeserializeFailure, log);

    #endregion
  }
}