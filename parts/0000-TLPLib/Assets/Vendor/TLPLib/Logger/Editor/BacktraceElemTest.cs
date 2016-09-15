using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Logger {
  [TestFixture]
  class BacktraceElemTest {
    [Test]
    public void testParseUnityBacktraceLine() {
      Assert.AreEqual(
        new BacktraceElem(
          "UnityEngine.Debug:LogError", "ProbablyWontHappen(Object)",
          F.none<BacktraceElem.FileInfo>()
        ),
        BacktraceElem.parseUnityBacktraceLine("UnityEngine.Debug:LogError:ProbablyWontHappen(Object)")
      );
      Assert.AreEqual(
        new BacktraceElem("UnityEngine.Debug", "LogError(Object)", F.none<BacktraceElem.FileInfo>()),
        BacktraceElem.parseUnityBacktraceLine("UnityEngine.Debug:LogError(Object)")
      );
      Assert.AreEqual(
        new BacktraceElem(
          "com.tinylabproductions.TLPLib.Logger.Log", "error(Object)",
          F.some(new BacktraceElem.FileInfo("Assets/Vendor/TLPLib/Logger/Log.cs", 14))
        ), 
        BacktraceElem.parseUnityBacktraceLine(
          "com.tinylabproductions.TLPLib.Logger.Log:error(Object) (at Assets/Vendor/TLPLib/Logger/Log.cs:14)"
        )
      );
      Assert.AreEqual(
        new BacktraceElem(
          "Assets.Code.Main", "<Awake>m__32()",
          F.some(new BacktraceElem.FileInfo("Assets/Code/Main.cs", 60))
        ), 
        BacktraceElem.parseUnityBacktraceLine(
          "Assets.Code.Main:<Awake>m__32() (at Assets/Code/Main.cs:60)"
        )
      );
      Assert.AreEqual(
        new BacktraceElem(
          "com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF", "MoveNext()",
          F.some(new BacktraceElem.FileInfo("Assets/Vendor/TLPLib/Concurrent/ASync.cs", 175))
        ), 
        BacktraceElem.parseUnityBacktraceLine(
          "com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext() (at Assets/Vendor/TLPLib/Concurrent/ASync.cs:175)"
        )
      );
    }
  }
}