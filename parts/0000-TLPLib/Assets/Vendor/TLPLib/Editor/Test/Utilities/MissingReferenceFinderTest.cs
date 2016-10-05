using com.tinylabproductions.TLPLib.Utilities.Editor;
using NUnit.Framework;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Test.Utilities {
  [TestFixture]
  public class MissingReferenceFinderTest {
    [Test]
    public void NullReference() {
      var ga = new GameObject();
      var comp = ga.AddComponent<Component>();
      var errors = ReferencesInPrefabs.findMissingReferences("", new [] { ga }, false);
      var actual = errors.Count;
      var expected = 1;
      Assert.AreEqual(expected, actual);
    }
  }
}
