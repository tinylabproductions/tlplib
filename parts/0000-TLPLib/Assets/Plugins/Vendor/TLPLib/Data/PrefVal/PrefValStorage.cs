using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure =
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log = null
    ) => collection(
      key, rw,
      ImmutableArray.CreateBuilder<A>, (b, a) => b.Add(a), b => b.MoveToImmutable(),
      defaultVal,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    public PrefVal<ImmutableList<A>> list<A>(
      string key, ISerializedRW<A> rw,
      ImmutableList<A> defaultVal = null, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure =
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log = null
    ) => collection(
      key, rw,
      ImmutableList.CreateBuilder<A>, (b, a) => b.Add(a), b => b.ToImmutable(),
      defaultVal ?? ImmutableList<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    public PrefVal<ImmutableHashSet<A>> hashSet<A>(
      string key, ISerializedRW<A> rw,
      ImmutableHashSet<A> defaultVal = null, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure =
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log = null
    ) => collection(
      key, rw,
      ImmutableHashSet.CreateBuilder<A>, (b, a) => b.Add(a), b => b.ToImmutable(),
      defaultVal ?? ImmutableHashSet<A>.Empty,
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

    #region Base64

    public PrefVal<A> base64<A>(
      string key, A defaultVal,
      Fn<A, IEnumerable<byte[]>> serialize,
      Fn<byte[][], Option<A>> deserialize,
      bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log=null
    ) {
      const char separator = '|';
      return custom(
        key, defaultVal,
        a => serialize(a).Select(Convert.ToBase64String).mkString(separator),
        base64Str => {
          try {
            // Split on empty string gives an array with 1 empty string
            var parts = base64Str == "" ? new byte[][]{} : base64Str.Split(separator).map(Convert.FromBase64String);
            return deserialize(parts);
          }
          catch (FormatException) {
            return Option<A>.None;
          }
        },
        saveOnEveryWrite: saveOnEveryWrite,
        onDeserializeFailure: onDeserializeFailure,
        log: log
      );
    }

    /* Custom mapping with Base64 strings as backing storage. */
    [Obsolete]
    public PrefVal<A> base64__OLD<A>(
      string key, A defaultVal, 
      Act<A, Act<string>> store, Fn<IEnumerable<string>, A> read,
      bool saveOnEveryWrite = true
    ) {
      return custom__OLD(
        key, defaultVal, 
        a => {
          var sb = new StringBuilder();
          var idx = 0;
          Act<string> storer = value => {
            if (idx != 0) sb.Append('|');
            sb.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
            idx++;
          };
          store(a, storer);
          return sb.ToString();
        },
        str => read(
          str.Split('|').Select(s => Encoding.UTF8.GetString(Convert.FromBase64String(s)))
        ),
        saveOnEveryWrite: saveOnEveryWrite
      );
    }

    #endregion

    #region Custom Collection

    static string deserializeCollectionItemFailureMsg<A, C>(
      string key, byte[] partData, string ending = ""
    ) =>
      $"Can't deserialize {typeof(A)} from '{BitConverter.ToString(partData)}' " +
      $"for PrefVal<{typeof(C)}> '{key}'{ending}.";

    public PrefVal<C> collection<A, C, CB>(
      string key,
      ISerializedRW<A> rw,
      Fn<CB> createCollectionBuilder, Act<CB, A> addToCollection, 
      Fn<CB, C> builderToCollection,
      C defaultVal, bool saveOnEveryWrite = true, 
      PrefVal.OnDeserializeFailure onDeserializeFailure = 
        PrefVal.OnDeserializeFailure.ReturnDefault,
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure = 
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log=null
    ) where C : IEnumerable<A> {
      log = log ?? Log.defaultLogger;
      return base64(
        key, defaultVal,
        c => c.Select(rw.serialize),
        parts => {
          var b = createCollectionBuilder();
          foreach (var partData in parts) {
            var deserialized = rw.deserialize(partData, 0);
            if (deserialized.isDefined) addToCollection(b, deserialized.get);
            else {
              if (
                onDeserializeCollectionItemFailure == 
                PrefVal.OnDeserializeCollectionItemFailure.Ignore
              ) { 
                if (log.isWarn())
                  log.warn(deserializeCollectionItemFailureMsg<A, C>(key, partData, ", ignoring"));
              }
              else throw new SerializationException(
                deserializeCollectionItemFailureMsg<A, C>(key, partData)
              );
            }
          }
          return builderToCollection(b).some();
        },
        saveOnEveryWrite: saveOnEveryWrite,
        onDeserializeFailure: onDeserializeFailure,
        log: log
      );
    }

    #endregion
  }
}