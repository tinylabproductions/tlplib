using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    uint getUInt(string name, uint defaultValue);
    void setInt(string name, int value);
    void setUint(string name, uint value);
    float getFloat(string name, float defaultValue);
    void setFloat(string name, float value);
    void save();
  }

  class PlayerPrefsBackend : IPrefValueBackend {
    public static readonly PlayerPrefsBackend instance = new PlayerPrefsBackend();
    PlayerPrefsBackend() {}

    public string getString(string name, string defaultValue) { return PlayerPrefs.GetString(name, defaultValue); }
    public void setString(string name, string value) { PlayerPrefs.SetString(name, value); }
    public int getInt(string name, int defaultValue) { return PlayerPrefs.GetInt(name, defaultValue); }
    public uint getUInt(string name, uint defaultValue) { return unchecked((uint)PlayerPrefs.GetInt(name, unchecked((int)defaultValue))); }
    public void setInt(string name, int value) { PlayerPrefs.SetInt(name, value); }
    public void setUint(string name, uint value) { PlayerPrefs.SetInt(name, unchecked((int)value)); }
    public float getFloat(string name, float defaultValue) { return PlayerPrefs.GetFloat(name, defaultValue); }
    public void setFloat(string name, float value) { PlayerPrefs.SetFloat(name, value); }
    public void save() { PlayerPrefs.Save(); }
  }

#if UNITY_EDITOR
  class EditorPrefsBackend : IPrefValueBackend {
    public static readonly EditorPrefsBackend instance = new EditorPrefsBackend();
    EditorPrefsBackend() {}

    public string getString(string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }
    public void setString(string name, string value) { EditorPrefs.SetString(name, value); }
    public int getInt(string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }
    public uint getUInt(string name, uint defaultValue) { return unchecked((uint)EditorPrefs.GetInt(name, unchecked((int)defaultValue))); }
    public void setInt(string name, int value) { EditorPrefs.SetInt(name, value); }
    public void setUint(string name, uint value) { EditorPrefs.SetInt(name, unchecked((int)value)); }
    public float getFloat(string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }
    public void setFloat(string name, float value) { EditorPrefs.SetFloat(name, value); }
    public void save() {}
  }
#endif

  public class PrefValStorage {
    /* If you store this as a value in type custom PrefValue, you'll get back a default value. */
    public const string CUSTOM_DEFAULT = "";

    readonly IPrefValueBackend backend;

    public PrefValStorage(IPrefValueBackend backend) { this.backend = backend; }

    public PrefVal<string> str(string key, string defaultVal, bool saveOnEveryWrite=true) => 
      new PrefVal<string>(
        () => backend.getString(key, defaultVal), 
        value => backend.setString(key, value), backend, saveOnEveryWrite
      );

    public PrefVal<int> integer(string key, int defaultVal, bool saveOnEveryWrite=true) => 
      new PrefVal<int>(
        () => backend.getInt(key, defaultVal), 
        value => backend.setInt(key, value), backend, saveOnEveryWrite
      );

    public PrefVal<uint> uinteger(string key, uint defaultVal, bool saveOnEveryWrite=true) => 
      new PrefVal<uint>(
        () => backend.getUInt(key, defaultVal), 
        value => backend.setUint(key, value), 
        backend, saveOnEveryWrite
      );

    public PrefVal<float> flt(string key, float defaultVal, bool saveOnEveryWrite=true) => 
      new PrefVal<float>(
        () => backend.getFloat(key, defaultVal), 
        value => backend.setFloat(key, value), 
        backend, saveOnEveryWrite
      );

    #region bool

    public PrefVal<bool> boolean(string key, bool defaultVal, bool saveOnEveryWrite = true) => 
      new PrefVal<bool>(
        () => backend.getInt(key, bool2int(defaultVal)) != 0, 
        value => backend.setInt(key, bool2int(value)),
        backend, saveOnEveryWrite
      );

    static int bool2int(bool b) => b ? 1 : 0;

    #endregion

    #region Duration

    public PrefVal<Duration> duration(string key, Duration defaultVal, bool saveOnEveryWrite=true) => 
      new PrefVal<Duration>(
        () => new Duration(backend.getInt(key, defaultVal.millis)),
        value => backend.setInt(key, value.millis),
        backend, saveOnEveryWrite
      );

    #endregion

    #region DateTime

    public PrefVal<DateTime> dateTime(string key, DateTime defaultVal, bool saveOnEveryWrite = true) => 
      new PrefVal<DateTime>(
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
    public PrefVal<A> custom<A>(
      string key, A defaultVal, Fn<A, string> map, Fn<string, A> comap, bool saveOnEveryWrite=true
    ) => new PrefVal<A>(
      () => GetCustom(key, defaultVal, comap),
      value => SetCustom(key, value, map),
      backend, saveOnEveryWrite
    );

    A GetCustom<A>(string key, A defaultVal, Fn<string, A> parse) {
      var str = backend.getString(key, CUSTOM_DEFAULT);
      return str == CUSTOM_DEFAULT ? defaultVal : parse(str);
    }

    void SetCustom<A>(string key, A value, Fn<A, string> serialize)
    { backend.setString(key, serialize(value)); }

    #endregion

    /* Custom mapping with Base64 strings as backing storage. */
    public PrefVal<A> base64<A>(
      string key, A defaultVal, Act<A, Act<string>> store, Fn<IEnumerable<string>, A> read,
      bool saveOnEveryWrite = true
    ) {
      return custom(
        key, defaultVal, a => {
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
        str => read(str.Split('|').Select(b64 =>
          Encoding.UTF8.GetString(Convert.FromBase64String(b64))
        )),
        saveOnEveryWrite: saveOnEveryWrite
      );
    }
  }

  // Should be class (not struct) because .write mutates object.
  public class PrefVal<A> {
    readonly IPrefValueBackend backend;
    readonly Act<A> writer;
    readonly bool saveOnEveryWrite;

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

    public PrefVal(
      Fn<A> reader, Act<A> writer,
      IPrefValueBackend backend, bool saveOnEveryWrite
    ) {
      this.writer = writer;
      this.backend = backend;
      this.saveOnEveryWrite = saveOnEveryWrite;
      _value = persist(reader());
    }

    public A read => value;

    public A write(A value) {
      this.value = value;
      return value;
    }

    // You should not write to Val when using RxRef
    public RxRef<A> toRxRef() {
      var rx = new RxRef<A>(read);
      rx.subscribe(v => write(v));
      return rx;
    }

    public void forceSave() {
      backend.save();
    }

    public override string ToString() => $"{nameof(PrefVal<A>)}({_value})";
  }

  /* PlayerPrefs backed reactive value. */
  public static class PrefVal {
    public static readonly PrefValStorage player = new PrefValStorage(PlayerPrefsBackend.instance);
#if UNITY_EDITOR
    public static readonly PrefValStorage editor = new PrefValStorage(EditorPrefsBackend.instance);
#endif
  }
}
