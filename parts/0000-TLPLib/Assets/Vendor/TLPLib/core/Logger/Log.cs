using System;
using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.config;
using pzd.lib.functional;
using pzd.lib.serialization;
using pzd.lib.typeclasses;
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
    public enum Level : byte { VERBOSE = 10, DEBUG = 20, INFO = 30, WARN = 40, ERROR = 50 }
    public static class Level_ {
      public static readonly ISerializedRW<Level> rw = 
        SerializedRW.byte_.map<byte, Level>(b => (Level) b, l => (byte) l);

      public static readonly Config.Parser<object, Level> parser =
        Config.byteParser.flatMap((_, b) => EnumUtils.GetValues<Level>().find(l => (byte) l == b));
    }

    // InitializeOnLoad is needed to set static variables on main thread.
    // FKRs work without it, but on Gummy Bear repo tests fail
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod]
    static void init() {}

    public static readonly Level defaultLogLevel =
      Application.isEditor || Debug.isDebugBuild
      ? Level.DEBUG : Level.INFO;

    static readonly bool useConsoleLog = EditorUtils.inBatchMode;

    static Log() {
      DConsole.instance.registrarOnShow(
        NeverDisposeDisposableTracker.instance, "Default Logger",
        (dc, r) => {
          r.registerEnum(
            "level",
            Ref.a(() => @default.level, v => @default.level = v),
            EnumUtils.GetValues<Level>()
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

  [Record] public readonly partial struct LogEvent {
    public readonly Log.Level level;
    public readonly LogEntry entry;
  }
}
