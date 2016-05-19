using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Editor.Test.Functional {
  class TestException : Exception {}

  public class TryTestMap {
    static readonly TestException ex = new TestException();

    [Test]
    public void ErrorToGood() {
      new Try<int>(ex).map(a => a * 2).shouldBeError(ex.GetType());
    }

    [Test]
    public void ErrorToError() {
      new Try<int>(ex).map<int>(a => { throw new Exception(); }).shouldBeError(ex.GetType());
    }

    [Test]
    public void GoodToGood() {
      new Try<int>(1).map(a => a * 2).shouldBeSuccess(2);
    }

    [Test]
    public void GoodToError() {
      new Try<int>(1).map<int>(a => { throw ex; }).shouldBeError(ex.GetType());
    }
  }
}
