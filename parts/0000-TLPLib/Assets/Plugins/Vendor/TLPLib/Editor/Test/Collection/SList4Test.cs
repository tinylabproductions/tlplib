using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Collection {
  public class SList4Test {
    [Test]
    public void TestCreate() {
      SList4.create(-1).shouldEqual<IList<int>>(F.list(-1));
      SList4.create(-1, -2).shouldEqual<IList<int>>(F.list(-1, -2));
      SList4.create(-1, -2, -3).shouldEqual<IList<int>>(F.list(-1, -2, -3));
      SList4.create(-1, -2, -3, -4).shouldEqual<IList<int>>(F.list(-1, -2, -3, -4));
      SList4.create(-1, -2, -3, -4, -5).shouldEqual<IList<int>>(F.list(-1, -2, -3, -4, -5));
      SList4.create(-1, -2, -3, -4, -5, -6).shouldEqual<IList<int>>(F.list(-1, -2, -3, -4, -5, -6));
    }

    [Test]
    public void TestIndexing() => IListDefaultImplsTest.testIndexing(new SList4<int>());

    [Test]
    public void TestCount() => IListDefaultImplsTest.testCount(new SList4<int>());

    [Test]
    public void TestClear() => IListDefaultImplsTest.testClear(new SList4<int>());

    [Test]
    public void TestAdd() => IListDefaultImplsTest.testAdd(new SList4<int>());

    [Test]
    public void TestRemoveAt() => IListDefaultImplsTest.testRemoveAt(new SList4<int>());

    [Test]
    public void TestIndexOf() => IListDefaultImplsTest.testIndexOf(new SList4<int>());

    [Test]
    public void TestContains() => IListDefaultImplsTest.testContains(new SList4<int>());

    [Test]
    public void TestRemove() => IListDefaultImplsTest.testRemove(new SList4<int>());

    [Test]
    public void TestInsert() => IListDefaultImplsTest.testInsert(new SList4<char>());

    [Test]
    public void TestCopyTo() => IListDefaultImplsTest.testCopyTo(new SList4<int>());

    [Test]
    public void TestForeach() => IListDefaultImplsTest.testForeach(new SList4<int>());
  }
}