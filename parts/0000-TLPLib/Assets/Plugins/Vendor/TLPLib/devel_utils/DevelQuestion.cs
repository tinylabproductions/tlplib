using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.devel_utils {
  public static class DevelQuestion {
#if UNITY_EDITOR
    public static PrefVal<bool> enabledPrefVal = PrefVal.editor.boolean(nameof(DevelQuestion), false);
#endif

    public static bool enabled
#if UNITY_EDITOR
        = enabledPrefVal.value
#endif
      ;

#if UNITY_EDITOR
    static DevelQuestion() {
      DConsole.instance.onShow += dc => {
        var r = dc.registrarFor(nameof(DevelQuestion));
        r.registerToggle("PrefVal enabled", enabledPrefVal);
        r.registerToggle("enabled", () => enabled, v => enabled = v);
      };
    }
#endif

    public static bool askDeveloper(
      string title, bool defaultValue, string info = "",
      string yesButtonText = "Yes", string noButtonText = "No"
    ) {
      if (Log.d.isDebug()) Log.d.debug(
        $"{nameof(DevelQuestion)}#{nameof(askDeveloper)}[" +
        $"{nameof(enabled)}={enabled}, " +
        $"{nameof(title)}={title}, {nameof(defaultValue)}={defaultValue}, " +
        $"{nameof(info)}={info}, {nameof(yesButtonText)}={yesButtonText}, " +
        $"{nameof(noButtonText)}={noButtonText}" +
        $"]"
      );
#if UNITY_EDITOR
      return enabled 
        ? UnityEditor.EditorUtility.DisplayDialog(title, info, yesButtonText, noButtonText)
        : defaultValue;
#else
      return defaultValue;
#endif
    }
  }
}