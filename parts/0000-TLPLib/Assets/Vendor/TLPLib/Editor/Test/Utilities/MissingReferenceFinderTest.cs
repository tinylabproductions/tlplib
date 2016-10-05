using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.Utilities.Editor;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.Test.Utilities {
  [TestFixture]
  public class MissingReferenceFinderTest {
    class TestClass : MonoBehaviour {
      public GameObject field;
    }

    class NotNullClass : MonoBehaviour {
      [NotNull] public GameObject field;
    }

    class CanBeNullClass : MonoBehaviour {
      [CanBeNull] public GameObject field;
      public GameObject field2;
    }

    [Test]
    public void MissingComponent() {
      var go = new GameObject();
      var testClass = go.AddComponent<TestClass>();
      testClass.field = new GameObject();
      Object.DestroyImmediate(testClass.field);
      var errors = ReferencesInPrefabs.findMissingReferences("", new [] { go }, false);
      errors.shouldNotBeEmpty();
    }

    [Test]
    public void NullField() {
      var go = new GameObject();
      go.AddComponent<NotNullClass>();
      var errors = ReferencesInPrefabs.findMissingReferences("", new [] { go }, false);
      errors.shouldNotBeEmpty();
    }

    [Test]
    public void CanBeNullField() {
      var go = new GameObject();
      go.AddComponent<CanBeNullClass>();
      var errors = ReferencesInPrefabs.findMissingReferences("", new [] { go }, false);
      errors.shouldBeEmpty();
    }
  }
}
