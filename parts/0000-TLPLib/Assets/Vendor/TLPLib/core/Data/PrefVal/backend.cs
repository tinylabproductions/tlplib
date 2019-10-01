using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Data {
  public interface IPrefValueBackend {
    bool hasKey(string name);
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
    // Uint has the same width as int.
    // This way we can store large uint which is stored as negative int.
    // And when we want to get it, we just cast it back to uint.
    // uncheked because we do not store more than MAX_UINT value.
    public static uint getUInt(
      this IPrefValueBackend backend, string name, uint defaultValue
    ) =>
      unchecked((uint)backend.getInt(name, unchecked((int)defaultValue)));

    public static void setUInt(
      this IPrefValueBackend backend, string name, uint value
    ) =>
      backend.setInt(name, unchecked((int)value));

    public static bool getBool(
      this IPrefValueBackend backend, string name, bool defaultValue
    ) => backend.getInt(name, bool2int(defaultValue)) != 0;

    public static void setBool(
      this IPrefValueBackend backend, string name, bool value
    ) => backend.setInt(name, bool2int(value));

    static int bool2int(bool b) => b ? 1 : 0;
  }

  [PublicAPI] public sealed class ScopedPrefValueBackend : IPrefValueBackend {
    public readonly IPrefValueBackend backend;
    public readonly string scope;

    public ScopedPrefValueBackend(IPrefValueBackend backend, string scope) {
      this.backend = backend;
      this.scope = scope;
    }

    public string key(string name) => $"{scope}{name}";

    public bool hasKey(string name) => backend.hasKey(key(name));
    public string getString(string name, string defaultValue) => backend.getString(key(name), defaultValue);
    public void setString(string name, string value) => backend.setString(key(name), value);
    public int getInt(string name, int defaultValue) => backend.getInt(key(name), defaultValue);
    public void setInt(string name, int value) => backend.setInt(key(name), value);
    public float getFloat(string name, float defaultValue) => backend.getFloat(key(name), defaultValue);
    public void setFloat(string name, float value) => backend.setFloat(key(name), value);
    public void save() => backend.save();
    public void delete(string name) => backend.delete(key(name));
  }

  class PlayerPrefsBackend : IPrefValueBackend {
    public static readonly PlayerPrefsBackend instance = new PlayerPrefsBackend();
    PlayerPrefsBackend() {}

    public bool hasKey(string name) => PlayerPrefs.HasKey(name);
    public string getString(string name, string defaultValue) => PlayerPrefs.GetString(name, defaultValue);
    public void setString(string name, string value) => PlayerPrefs.SetString(name, value);
    public int getInt(string name, int defaultValue) => PlayerPrefs.GetInt(name, defaultValue);
    public void setInt(string name, int value) => PlayerPrefs.SetInt(name, value);
    public float getFloat(string name, float defaultValue) => PlayerPrefs.GetFloat(name, defaultValue);
    public void setFloat(string name, float value) => PlayerPrefs.SetFloat(name, value);
    public void save() => PlayerPrefs.Save();
    public void delete(string name) => PlayerPrefs.DeleteKey(name);
  }

  [PublicAPI] public sealed class InMemoryPlayerPrefsBackend : IPrefValueBackend {
    readonly Dictionary<string, string> strings = new Dictionary<string, string>();
    readonly Dictionary<string, int> ints = new Dictionary<string, int>();
    readonly Dictionary<string, float> floats = new Dictionary<string, float>();

    public bool hasKey(string name) => 
      strings.ContainsKey(name) || ints.ContainsKey(name) || floats.ContainsKey(name);

    public string getString(string name, string defaultValue) =>
      strings.TryGetValue(name, out var v) ? v : defaultValue;

    public void setString(string name, string value) => strings[name] = value;

    public int getInt(string name, int defaultValue) =>
      ints.TryGetValue(name, out var v) ? v : defaultValue;

    public void setInt(string name, int value) => ints[name] = value;

    public float getFloat(string name, float defaultValue) =>
      floats.TryGetValue(name, out var v) ? v : defaultValue;

    public void setFloat(string name, float value) => floats[name] = value;

    public void save() {}

    public void delete(string name) {
      if (strings.Remove(name)) return;
      if (ints.Remove(name)) return;
      floats.Remove(name);
    }
  }
  
#if UNITY_EDITOR
  sealed class EditorPrefsBackend : IPrefValueBackend {
    public static readonly EditorPrefsBackend instance = new EditorPrefsBackend();
    EditorPrefsBackend() {}

    public bool hasKey(string name) => EditorPrefs.HasKey(name);
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
}