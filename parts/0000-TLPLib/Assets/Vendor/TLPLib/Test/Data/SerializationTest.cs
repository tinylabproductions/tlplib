using System;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;
using pzd.lib.serialization;
using pzd.lib.test.serialization;

namespace com.tinylabproductions.TLPLib.Data {
  public class SerializationTestTplRW : SerializationTestBase {
    static readonly ISerializedRW<Tpl<int, string>> rw =
      SerializedRW.integer.tpl(SerializedRW.str);

    [Test] public void TestTpl() {
      var t = F.t(1, "foo");
      var serialized = rw.serializeToArray(t);
      checkWithNoise(rw, serialized, t);
    }
  }
}