﻿#if UNITY_ANDROID
using NUnit.Framework;
using pzd.lib.test_framework;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class StackTraceElementTest {
    [Test]
    public void TestMethodName() {
      "com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar+<>c__DisplayClass9_0`2[com.tinylabproductions.TLPLib.Functional.Unit,com.tinylabproductions.TLPLib.Functional.Unit].<register>b__0 ()"
        .methodAsAndroid().shouldEqual(
          "com.tinylabproductions.TLPLib.Components.DebugConsole.DConsoleRegistrar$$$c__DisplayClass9_0$2$com.tinylabproductions.TLPLib.Functional.Unit$com.tinylabproductions.TLPLib.Functional.Unit$.$register$b__0$$$"
        );
    }
  }
}
#endif