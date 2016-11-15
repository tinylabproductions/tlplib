using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using Smooth.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Data {
  public interface IPrefValueBackend {
    string getString(string name, string defaultValue);
    void setString(string name, string value);
    int getInt(string name, int defaultValue);
    void setInt(string name, int value);
    float getFloat(string name, float defaultValue);
    void setFloat(string name, float value);
    void save();
    void delete(string name);
  }

  public static class IPrefValueBackendExts {
    public static uint getUInt(
      this IPrefValueBackend backend, string name, uint defaultValue
    ) => 
      unchecked((uint)backend.getInt(name, unchecked((int)defaultValue)));

    public static void setUInt(
      this IPrefValueBackend backend, string name, uint value
    ) =>
      backend.setInt(name, unchecked((int)value));
  }

  class PlayerPrefsBackend : IPrefValueBackend {
    public static readonly PlayerPrefsBackend instance = new PlayerPrefsBackend();
    PlayerPrefsBackend() {}

    public string getString(string name, string defaultValue) => PlayerPrefs.GetString(name, defaultValue);
    public void setString(string name, string value) => PlayerPrefs.SetString(name, value);
    public int getInt(string name, int defaultValue) => PlayerPrefs.GetInt(name, defaultValue);
    public void setInt(string name, int value) => PlayerPrefs.SetInt(name, value);
    public float getFloat(string name, float defaultValue) => PlayerPrefs.GetFloat(name, defaultValue);
    public void setFloat(string name, float value) => PlayerPrefs.SetFloat(name, value);
    public void save() => PlayerPrefs.Save();
    public void delete(string name) => PlayerPrefs.DeleteKey(name);
  }

#if UNITY_EDITOR
  class EditorPrefsBackend : IPrefValueBackend {
    public static readonly EditorPrefsBackend instance = new EditorPrefsBackend();
    EditorPrefsBackend() {}

    public string getString(string name, string defaultValue) => EditorPrefs.GetString(name, defaultValue);
    public void setString(string name, string value) => EditorPrefs.SetString(name, value);
    public int getInt(string name, int defaultValue) => EditorPrefs.GetInt(name, defaultValue);
    public void setInt(string name, int value) => EditorPrefs.SetInt(name, value);
    public float getFloat(string name, float defaultValue) => EditorPrefs.GetFloat(name, defaultValue);
    public void setFloat(string name, float value) => EditorPrefs.SetFloat(name, value);
    public void save() {}
    public void delete(string name) => EditorPrefs.DeleteKey(name);
  }
#endif

  public class PrefValStorage {
    /* If you store this as a value in type custom PrefValue, you'll get back a default value. */
    public const string CUSTOM_V1_DEFAULT = "";

    readonly IPrefValueBackend backend;

    public PrefValStorage(IPrefValueBackend backend) { this.backend = backend; }

    public PrefVal<string> str(string key, string defaultVal, bool saveOnEveryWrite=true) => 
      new PrefValImpl<string>(
        key,
        () => backend.getString(key, defaultVal), 
        value => backend.setString(key, value), backend, saveOnEveryWrite
      );

    public PrefVal<int> integer(string key, int defaultVal, bool saveOnEveryWrite=true) => 
      new PrefValImpl<int>(
        key,
        () => backend.getInt(key, defaultVal), 
        value => backend.setInt(key, value), backend, saveOnEveryWrite
      );

    public PrefVal<uint> uinteger(string key, uint defaultVal, bool saveOnEveryWrite=true) => 
      new PrefValImpl<uint>(
        key,
        () => backend.getUInt(key, defaultVal), 
        value => backend.setUInt(key, value), 
        backend, saveOnEveryWrite
      );

    public PrefVal<float> flt(string key, float defaultVal, bool saveOnEveryWrite=true) => 
      new PrefValImpl<float>(
        key,
        () => backend.getFloat(key, defaultVal), 
        value => backend.setFloat(key, value), 
        backend, saveOnEveryWrite
      );

    #region bool

    public PrefVal<bool> boolean(string key, bool defaultVal, bool saveOnEveryWrite = true) => 
      new PrefValImpl<bool>(
        key,
        () => backend.getInt(key, bool2int(defaultVal)) != 0, 
        value => backend.setInt(key, bool2int(value)),
        backend, saveOnEveryWrite
      );

    static int bool2int(bool b) => b ? 1 : 0;

    #endregion

    #region Collections

    public PrefVal<ImmutableArray<A>> array<A>(
      string key,
      Fn<A, byte[]> serialize, Fn<byte[], Option<A>> deserialize,
      ImmutableArray<A> defaultVal, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure =
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log = null
    ) => collection(
      key, serialize, deserialize,
      ImmutableArray.CreateBuilder<A>, (b, a) => b.Add(a), b => b.MoveToImmutable(),
      defaultVal,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    public PrefVal<ImmutableList<A>> list<A>(
      string key,
      Fn<A, byte[]> serialize, Fn<byte[], Option<A>> deserialize,
      ImmutableList<A> defaultVal = null, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure =
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log = null
    ) => collection(
      key, serialize, deserialize,
      ImmutableList.CreateBuilder<A>, (b, a) => b.Add(a), b => b.ToImmutable(),
      defaultVal ?? ImmutableList<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    public PrefVal<ImmutableHashSet<A>> hashSet<A>(
      string key,
      Fn<A, byte[]> serialize, Fn<byte[], Option<A>> deserialize,
      ImmutableHashSet<A> defaultVal = null, bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault,
      PrefVal.OnDeserializeCollectionItemFailure onDeserializeCollectionItemFailure =
        PrefVal.OnDeserializeCollectionItemFailure.Ignore,
      ILog log = null
    ) => collection(
      key, serialize, deserialize,
      ImmutableHashSet.CreateBuilder<A>, (b, a) => b.Add(a), b => b.ToImmutable(),
      defaultVal ?? ImmutableHashSet<A>.Empty,
      saveOnEveryWrite, onDeserializeFailure, onDeserializeCollectionItemFailure, log
    );

    #endregion

    #region Duration

    public PrefVal<Duration> duration(string key, Duration defaultVal, bool saveOnEveryWrite=true) => 
      new PrefValImpl<Duration>(
        key,
        () => new Duration(backend.getInt(key, defaultVal.millis)),
        value => backend.setInt(key, value.millis),
        backend, saveOnEveryWrite
      );

    #endregion

    #region DateTime

    public PrefVal<DateTime> dateTime(string key, DateTime defaultVal, bool saveOnEveryWrite = true) => 
      new PrefValImpl<DateTime>(
        key,
        () => deserializeDate(backend.getString(key, serializeDate(defaultVal))),
        value => backend.setString(key, serializeDate(value)),
        backend, saveOnEveryWrite
      );

    static string serializeDate(DateTime date) => date.ToBinary().ToString();
    static DateTime deserializeDate(string s) => DateTime.FromBinary(long.Parse(s));

    #endregion

    #region Custom

    /* Provide custom mapping. It uses string representation inside and returns
     * default value if string is empty. */
    [Obsolete]
    public PrefVal<A> custom__OLD<A>(
      string key, A defaultVal, Fn<A, string> map, Fn<string, A> comap, bool saveOnEveryWrite=true
    ) => new PrefValImpl<A>(
      key,
      () => {
        var str = backend.getString(key, CUSTOM_V1_DEFAULT);
        return str == CUSTOM_V1_DEFAULT ? defaultVal : comap(str);
      },
      value => backend.setString(key, map(value)),
      backend, saveOnEveryWrite
    );

    static string deserializeFailureMsg<A>(string key, string serialized, string ending="") =>
      $"Can't deserialize {typeof(A)} from '{serialized}' for PrefVal '{key}'{ending}.";
    
    public PrefVal<A> custom<A>(
      string key, A defaultVal, Fn<A, string> serialize, Fn<string, Option<A>> deserialize,
      bool saveOnEveryWrite = true, 
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) {
      const string defaultValue = "d", nonDefaultValueDiscriminator = "_";
      log = log ?? Log.defaultLogger;

      return new PrefValImpl<A>(
        key,
        () => {
          var serialized = backend.getString(key, defaultValue);
          if (serialized == defaultValue) return defaultVal;
          else {
            var serializedWithoutDiscriminator = serialized.Substring(1);
            var opt = deserialize(serializedWithoutDiscriminator);
            if (opt.isDefined) return opt.get;

            if (onDeserializeFailure == PrefVal.OnDeserializeFailure.ReturnDefault) {
              if (log.isWarn()) log.warn(deserializeFailureMsg<A>(key, serialized, ", returning default"));
              return defaultVal;
            }

            throw new SerializationException(deserializeFailureMsg<A>(key, serialized));
          }
        },
        a => backend.setString(key, $"{nonDefaultValueDiscriminator}{serialize(a)}"),
        backend, saveOnEveryWrite: saveOnEveryWrite
      );
    }

    public PrefVal<A> custom<A>(
      string key, A defaultVal, Fn<A, byte[]> serialize, Fn<byte[], Option<A>> deserialize,
      bool saveOnEveryWrite = true,
      PrefVal.OnDeserializeFailure onDeserializeFailure = PrefVal.OnDeserializeFailure.ReturnDefault,
      ILog log = null
    ) => custom(
      key, defaultVal,
      a => Convert.ToBase64String(serialize(a)),
      s => {
        try {
          var bytes = Convert.FromBase64String(s);
          return deserialize(bytes);
        }
        catch (FormatException) {
          return Option<A>.None;
        }
      },
      saveOnEveryWrite: saveOnEveryWrite, onDeserializeFailure: onDeserializeFailure, log: log
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
      Fn<A, byte[]> serialize, Fn<byte[], Option<A>> deserialize,
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
        c => c.Select(a => serialize(a)),
        parts => {
          var b = createCollectionBuilder();
          foreach (var partData in parts) {
            var deserialized = deserialize(partData);
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

  public interface PrefVal<A> : Ref<A>, ICachedBlob<A> {
    void forceSave();
  }

  public static class PrefValExts {
    // You should not write to Val when using RxRef
    public static RxRef<A> toRxRef<A>(this PrefVal<A> val) {
      var rx = new RxRef<A>(val.value);
      rx.subscribe(v => val.value = v);
      return rx;
    }

    public static PrefVal<B> bimap<A, B>(
      this PrefVal<A> val, BiMapper<A, B> bimap
    ) => new PrefValMapper<A, B>(val, bimap);

    public static ICachedBlob<A> optToCachedBlob<A>(
      this PrefVal<Option<A>> val
    ) => new PrefValOptCachedBlob<A>(val);
  }

  // Should be class (not struct) because .write mutates object.
  public class PrefValImpl<A> : PrefVal<A> {
    public readonly bool saveOnEveryWrite;
    public readonly string key;

    readonly IPrefValueBackend backend;
    readonly Act<A> writer;

    A _value;
    public A value {
      get { return _value; }
      set {
        if (EqComparer<A>.Default.Equals(_value, value)) return;
        _value = persist(value);
      }
    }

    A persist(A value) {
      writer(value);
      if (saveOnEveryWrite) backend.save();
      return value;
    }

    public PrefValImpl(
      string key, Fn<A> reader, Act<A> writer,
      IPrefValueBackend backend, bool saveOnEveryWrite
    ) {
      this.key = key;
      this.writer = writer;
      this.backend = backend;
      this.saveOnEveryWrite = saveOnEveryWrite;
      _value = persist(reader());
    }

    public void forceSave() => backend.save();

    public override string ToString() => $"{nameof(PrefVal<A>)}({_value})";

    #region ICachedBlob

    public bool cached => true;
    Option<Try<A>> ICachedBlob<A>.read() => F.some(F.scs(value));

    public Try<Unit> store(A data) {
      value = data;
      return F.scs(F.unit);
    }

    public Try<Unit> clear() {
      backend.delete(key);
      return F.scs(F.unit);
    } 

    #endregion
  }

  class PrefValOptCachedBlob<A> : ICachedBlob<A> {
    readonly PrefVal<Option<A>> backing;

    public PrefValOptCachedBlob(PrefVal<Option<A>> backing) { this.backing = backing; }

    public bool cached => backing.value.isDefined;
    public Option<Try<A>> read() => backing.value.map(F.scs);
    public Try<Unit> store(A data) => backing.store(data.some());
    public Try<Unit> clear() => backing.store(Option<A>.None);
  }

  class PrefValMapper<A, B> : PrefVal<B> {
    readonly PrefVal<A> backing;
    readonly BiMapper<A, B> bimap;

    public PrefValMapper(PrefVal<A> backing, BiMapper<A, B> bimap) {
      this.backing = backing;
      this.bimap = bimap;
    }

    public bool cached => backing.cached;
    Option<Try<B>> ICachedBlob<B>.read() => backing.read().map(t => t.map(bimap.map));
    public Try<Unit> store(B data) => backing.store(bimap.comap(data));
    public Try<Unit> clear() => backing.clear();

    public B value {
      get { return bimap.map(backing.value); }
      set { backing.value = bimap.comap(value); }
    }

    public void forceSave() => backing.forceSave();
  }

  /* PlayerPrefs backed reactive value. */
  public static class PrefVal {
    public delegate void Base64StorePart(byte[] partData);
    public delegate byte[] Base64ReadPart();

    public enum OnDeserializeFailure { ReturnDefault, ThrowException }
    public enum OnDeserializeCollectionItemFailure { Ignore, ThrowException }

    public static readonly Fn<string, byte[]> stringSerialize = 
      s => Encoding.UTF8.GetBytes(s);
    public static readonly Fn<byte[], Option<string>> stringDeserialize =
      bytes => Encoding.UTF8.GetStringTry(bytes).value;

    public static readonly PrefValStorage player = new PrefValStorage(PlayerPrefsBackend.instance);
#if UNITY_EDITOR
    public static readonly PrefValStorage editor = new PrefValStorage(EditorPrefsBackend.instance);
#endif
  }
}
