using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Reactive;
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
  }

  class PlayerPrefsBackend : IPrefValueBackend {
    public static readonly PlayerPrefsBackend instance = new PlayerPrefsBackend();
    PlayerPrefsBackend() {}

    public string getString(string name, string defaultValue) { return PlayerPrefs.GetString(name, defaultValue); }
    public void setString(string name, string value) { PlayerPrefs.SetString(name, value); }
    public int getInt(string name, int defaultValue) { return PlayerPrefs.GetInt(name, defaultValue); }
    public void setInt(string name, int value) { PlayerPrefs.SetInt(name, value); }
    public float getFloat(string name, float defaultValue) { return PlayerPrefs.GetFloat(name, defaultValue); }
    public void setFloat(string name, float value) { PlayerPrefs.SetFloat(name, value); }
  }

#if UNITY_EDITOR
  class EditorPrefsBackend : IPrefValueBackend {
    public static readonly EditorPrefsBackend instance = new EditorPrefsBackend();
    EditorPrefsBackend() {}

    public string getString(string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }
    public void setString(string name, string value) { EditorPrefs.SetString(name, value); }
    public int getInt(string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }
    public void setInt(string name, int value) { EditorPrefs.SetInt(name, value); }
    public float getFloat(string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }
    public void setFloat(string name, float value) { EditorPrefs.SetFloat(name, value); }
  }
#endif

  public class PrefValStorage {
    /* If you store this as a value in type custom PrefValue, you'll get back a default value. */
    public const string CUSTOM_DEFAULT = "";

    readonly IPrefValueBackend backend;

    public PrefValStorage(IPrefValueBackend backend) { this.backend = backend; }

    public PrefVal<string> str(string key, string defaultVal) {
      return new PrefVal<string>(key, defaultVal, backend.getString, backend.setString);
    }

    public PrefVal<int> integer(string key, int defaultVal) {
      return new PrefVal<int>(key, defaultVal, backend.getInt, backend.setInt);
    }

    public PrefVal<float> flt(string key, float defaultVal) {
      return new PrefVal<float>(key, defaultVal, backend.getFloat, backend.setFloat);
    }

    #region bool

    public PrefVal<bool> boolean(string key, bool defaultVal) {
      return new PrefVal<bool>(key, defaultVal, GetBool, SetBool);
    }

    public bool GetBool(string key, bool defaultVal)
    { return int2bool(backend.getInt(key, bool2int(defaultVal))); }

    public void SetBool(string key, bool value)
    { backend.setInt(key, bool2int(value)); }

    static bool int2bool(int i) { return i != 0; }
    static int bool2int(bool b) { return b ? 1 : 0; }

    #endregion

    #region DateTime

    public PrefVal<DateTime> dateTime(string key, DateTime defaultVal) {
      return new PrefVal<DateTime>(key, defaultVal, GetDate, SetDate);
    }

    public DateTime GetDate(string key, DateTime defaultVal)
    { return deserializeDate(backend.getString(key, serializeDate(defaultVal))); }

    public void SetDate(string key, DateTime value)
    { backend.setString(key, serializeDate(value)); }

    static string serializeDate(DateTime date) {
      return date.ToBinary().ToString();
    }

    static DateTime deserializeDate(string s) {
      return DateTime.FromBinary(long.Parse(s));
    }

    #endregion

    #region Custom

    /* Provide custom mapping. It uses string representation inside and returns
     * default value if string is empty. */
    public PrefVal<A> custom<A>(
      string key, A defaultVal, Fn<A, string> map, Fn<string, A> comap
    ) {
      return new PrefVal<A>(
        key, defaultVal,
        (_key, _defaultVal) => GetCustom(_key, _defaultVal, comap),
        (_key, value) => SetCustom(key, value, map)
      );
    }

    A GetCustom<A>(string key, A defaultVal, Fn<string, A> parse) {
      var str = backend.getString(key, CUSTOM_DEFAULT);
      return str == CUSTOM_DEFAULT ? defaultVal : parse(str);
    }

    void SetCustom<A>(string key, A value, Fn<A, string> serialize)
    { backend.setString(key, serialize(value)); }

    #endregion

    /* Custom mapping with Base64 strings as backing storage. */
    public PrefVal<A> base64<A>(
      string key, A defaultVal, Act<A, Act<string>> store, Fn<IEnumerable<string>, A> read
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
        ))
      );
    }
  }

  // Should be class (not struct) because .write mutates object.
  public class PrefVal<A> {
    readonly string key;
    readonly Act<string, A> writer;

    A _value;
    public A value {
      get { return _value; }
      set {
        writer(key, value);
        _value = value;
        PlayerPrefs.Save();
      }
    }

    public PrefVal(
      string key, A defaultValue, Fn<string, A, A> reader, Act<string, A> writer
    ) {
      this.key = key;
      this.writer = writer;
      value = reader(key, defaultValue);
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
  }

  /* PlayerPrefs backed reactive value. */
  public static class PrefVal {
    public static readonly PrefValStorage player = new PrefValStorage(PlayerPrefsBackend.instance);
#if UNITY_EDITOR
    public static readonly PrefValStorage editor = new PrefValStorage(EditorPrefsBackend.instance);
#endif
  }
}
