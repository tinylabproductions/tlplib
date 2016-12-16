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

#if UNITY_EDITOR
  class EditorPrefsBackend : IPrefValueBackend {
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