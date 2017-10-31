using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace Utils {
  public static class PlayerPrefsExts {
    public static void SetIntAndSave(string name, int value) {
      if (Log.d.isDebug()) Log.d.debug($"Storing PP {name}={value}");
      PlayerPrefs.SetInt(name, value);
      PlayerPrefs.Save();
    }

    public static void SetFloatAndSave(string name, float value) {
      if (Log.d.isDebug()) Log.d.debug($"Storing PP {name}={value}");
      PlayerPrefs.SetFloat(name, value);
      PlayerPrefs.Save();
    }

    public static void SetStringAndSave(string name, string value) {
      if (Log.d.isDebug()) Log.d.debug($"Storing PP {name}={value}");
      PlayerPrefs.SetString(name, value);
      PlayerPrefs.Save();
    }
  }
}
