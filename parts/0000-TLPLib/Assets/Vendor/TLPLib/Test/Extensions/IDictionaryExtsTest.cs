﻿using System.Collections.Generic;
using pzd.lib.test_framework;
using NUnit.Framework;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Extensions {
  [TestFixture]
  public class IDictionaryExtsTestGetOrUpdate {
    [Test]
    public void Test() {
      var dictionary = new Dictionary<int,string>();
      dictionary.getOrUpdate(1, () => "one").shouldEqual("one");
      dictionary.getOrUpdate(1, () => "two").shouldEqual("one");
    }
  }
}