using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using pzd.lib.data;
using pzd.lib.log;
using pzd.lib.utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.tinylabproductions.TLPLib.Logger {
  /**
   * This double checks logging levels because string concatenation is more
   * expensive than boolean check.
   *
   * The general rule of thumb is that if your log object doesn't need any
   * processing you can call appropriate logging method by itself. If it does
   * need processing, you should use `if (Log.d.isDebug()) Log.d.debug("foo=" + foo);` style.
   **/
  [PublicAPI] public static class Log {
    // InitializeOnLoad is needed to set static variables on main thread.
    // FKRs work without it, but on Gummy Bear repo tests fail
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod]
    static void init() {}

    public static readonly LogLevel defaultLogLevel =
      Application.isEditor || Debug.isDebugBuild
      ? LogLevel.DEBUG : LogLevel.INFO;

    static readonly bool useConsoleLog = EditorUtils.inBatchMode;

    static Log() {
      DConsole.instance.registrarOnShow(
        NeverDisposeDisposableTracker.instance, "Default Logger",
        (dc, r) => {
          r.registerEnum(
            "level",
            Ref.a(() => @default.level, v => @default.level = v),
            EnumUtils.GetValues<LogLevel>()
          );
        }
      );
    }

    static ILog _default;
    public static ILog @default {
      get => _default ??= useConsoleLog ? (ILog) ConsoleLog.instance : UnityLog.instance;
      set => _default = value;
    }

    /// <summary>
    /// Shorthand for <see cref="Log.@default"/>. Allows <code><![CDATA[
    /// if (Log.d.isInfo) Log.d.info("foo");
    /// ]]></code> syntax.
    /// </summary>
    public static ILog d => @default;
  }
}
