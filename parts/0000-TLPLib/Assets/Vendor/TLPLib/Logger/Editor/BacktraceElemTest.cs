using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Logger {
  class BacktraceElemTestParseUnityBacktraceLine {
    BacktraceElem elem(string method) => new BacktraceElem(method, F.none<BacktraceElem.FileInfo>());

    [Test]
    public void Test1() {
      BacktraceElem.parseUnityBacktraceLine(
        "UnityEngine.Debug:LogError(Object)"
      ).shouldEqual(
        elem("UnityEngine.Debug:LogError(Object)")
      );
      BacktraceElem.parseUnityBacktraceLine(
        "com.tinylabproductions.TLPLib.Logger.Log:error(Object) (at Assets/Vendor/TLPLib/Logger/Log.cs:14)"
      ).shouldEqual(
        new BacktraceElem(
          "com.tinylabproductions.TLPLib.Logger.Log:error(Object)",
          F.some(new BacktraceElem.FileInfo("Assets/Vendor/TLPLib/Logger/Log.cs", 14))
        )
      );
      BacktraceElem.parseUnityBacktraceLine(
        "Assets.Code.Main:<Awake>m__32() (at Assets/Code/Main.cs:60)"
      ).shouldEqual(
        new BacktraceElem(
          "Assets.Code.Main:<Awake>m__32()",
          F.some(new BacktraceElem.FileInfo("Assets/Code/Main.cs", 60))
        )
      );
      BacktraceElem.parseUnityBacktraceLine(
        "com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext() (at Assets/Vendor/TLPLib/Concurrent/ASync.cs:175)"
      ).shouldEqual(
        new BacktraceElem(
          "com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext()",
          F.some(new BacktraceElem.FileInfo("Assets/Vendor/TLPLib/Concurrent/ASync.cs", 175))
        )
      );
    }

    [Test]
    public void Test2() {
      var actual = BacktraceElem.parseUnityBacktrace(
@"com.tinylabproductions.TLPGame.TLPGame+<>c.<.ctor>b__13_16 ()
com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass4_0.<register>b__0 ()
com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass5_0`1[com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 (Unit _)
com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass8_0`2[com.tinylabproductions.TLPLib.Functional.Unit,com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 (Unit obj)
com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass9_0`2[com.tinylabproductions.TLPLib.Functional.Unit,com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 ()
com.tinylabproductions.TLPLib.Components.DebugConsole.DConsole+<>c__DisplayClass18_0.<showGroup>b__0 ()
UnityEngine.Events.InvokableCall.Invoke (System.Object[] args)"
      );
      var expected = ImmutableList.Create(
        elem("com.tinylabproductions.TLPGame.TLPGame+<>c.<.ctor>b__13_16 ()"),
        elem("com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass4_0.<register>b__0 ()"),
        elem("com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass5_0`1[com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 (Unit _)"),
        elem("com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass8_0`2[com.tinylabproductions.TLPLib.Functional.Unit,com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 (Unit obj)"),
        elem("com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass9_0`2[com.tinylabproductions.TLPLib.Functional.Unit,com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 ()"),
        elem("com.tinylabproductions.TLPLib.Components.DebugConsole.DConsole+<>c__DisplayClass18_0.<showGroup>b__0 ()"),
        elem("UnityEngine.Events.InvokableCall.Invoke (System.Object[] args)")
      );
      actual.shouldEqual(expected);
    }
  }
}