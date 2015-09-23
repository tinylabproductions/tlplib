using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  /* PlayerPrefs backed reactive value. */
  public static class PrefValue {
    /* If you store this as a value in type custom PrefValue, you'll get back a default value. */
    public const string CUSTOM_DEFAULT = "";

    // Should be class (not struct) because .write mutates object.
    public class Val<A> {
      readonly string key;
      readonly Act<string, A> writer;
      A cache;

      public Val(
        string key, A defaultValue, Fn<string, A, A> read, Act<string, A> write
      ) {
        this.key = key;
        writer = write;
        cache = read(key, defaultValue);
      }

      public A read { get { return cache; } }

      public A write(A value) {
        writer(key, value); 
        cache = value;
        PlayerPrefs.Save();
        return value;
      }

      // You should not write to Val when using RxRef
      public RxRef<A> toRxRef() {
        var rx = new RxRef<A>(read);
        rx.subscribe(v => write(v));
        return rx;
      } 
    }

    public static Val<string> str(string key, string defaultVal) {
      return new Val<string>(key, defaultVal, PlayerPrefs.GetString, PlayerPrefs.SetString);
    }

    public static Val<int> integer(string key, int defaultVal) {
      return new Val<int>(key, defaultVal, PlayerPrefs.GetInt, PlayerPrefs.SetInt);
    }

    public static Val<float> flt(string key, float defaultVal) {
      return new Val<float>(key, defaultVal, PlayerPrefs.GetFloat, PlayerPrefs.SetFloat);
    }

    public static Val<bool> boolean(string key, bool defaultVal) {
      return new Val<bool>(key, defaultVal, GetBool, SetBool);
    }

    static bool GetBool(string key, bool defaultVal) 
    { return int2bool(PlayerPrefs.GetInt(key, bool2int(defaultVal))); }

    static void SetBool(string key, bool value) 
    { PlayerPrefs.SetInt(key, bool2int(value)); }

    static bool int2bool(int i) { return i != 0; }
    static int bool2int(bool b) { return b ? 1 : 0; }


    /* Provide custom mapping. It uses string representation inside and returns 
     * default value if string is empty. */
    public static Val<A> custom<A>(
      string key, A defaultVal, Fn<A, string> map, Fn<string, A> comap
    ) {
      return new Val<A>(
        key, defaultVal, 
        (_key, _defaultVal) => GetCustom(_key, _defaultVal, comap),
        (_key, value) => SetCustom(key, value, map)
      );
    }

    static A GetCustom<A>(string key, A defaultVal, Fn<string, A> parse) {
      var str = PlayerPrefs.GetString(key, CUSTOM_DEFAULT);
      return str == CUSTOM_DEFAULT ? defaultVal : parse(str);
    }

    static void SetCustom<A>(string key, A value, Fn<A, string> serialize) 
    { PlayerPrefs.SetString(key, serialize(value)); }

    /* Custom mapping with Base64 strings as backing storage. */
    public static Val<A> base64<A>(
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
}
