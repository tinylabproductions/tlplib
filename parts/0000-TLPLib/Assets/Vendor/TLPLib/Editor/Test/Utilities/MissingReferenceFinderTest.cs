using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.Utilities.Editor;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
// ReSharper disable ClassNeverInstantiated.Local, NotNullMemberIsNotInitialized
#pragma warning disable 169

namespace com.tinylabproductions.TLPLib.Editor.Test.Utilities {
  public class MissingReferenceFinderTest {
    class TestClass : MonoBehaviour {
      public GameObject field;
    }

    class NotNullPublicField : MonoBehaviour {
      [NotNull] public GameObject field;
    }

    class NotNullSerializedField : MonoBehaviour {
      [NotNull, SerializeField] GameObject field;
    }

    [Serializable]
    public struct InnerNotNull {
      [NotNull] public GameObject field;  
    }

    class NullReferencePublicField : MonoBehaviour {
      public InnerNotNull field = new InnerNotNull();
    }

    class NullReferenceSerializedField : MonoBehaviour {
      [SerializeField] InnerNotNull field = new InnerNotNull();
    }

    [Test] public void WhenMissingReference() => test<TestClass>(
      a => {
        a.field = new GameObject();
        Object.DestroyImmediate(a.field);
      },
      ReferencesInPrefabs.ErrorType.MISSING_REF.some()
    );
    [Test] public void WhenReferenceNotMissing() => test<TestClass>();
    [Test] public void WhenMissingReferenceInner() => Assert.Fail();
    [Test] public void WhenReferenceNotMissingInner() => Assert.Fail();

    [Test] public void WhenNotNullPublicField() => test<NotNullPublicField>(
      errorType: ReferencesInPrefabs.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNotNullPublicFieldSet() => Assert.Fail();
    [Test] public void WhenNotNullSerializedField() => test<NotNullSerializedField>(
      errorType: ReferencesInPrefabs.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNotNullSerializedFieldSet() => Assert.Fail();

    [Test] public void WhenNullInsideMonoBehaviorPublicField() => test<NullReferencePublicField>(
      errorType: ReferencesInPrefabs.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNullInsideMonoBehaviorPublicFieldSet() => Assert.Fail();
    [Test] public void WhenNullInsideMonoBehaviorSerializedField() => test<NullReferenceSerializedField>(
      errorType: ReferencesInPrefabs.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNullInsideMonoBehaviorSerializedFieldSet() => Assert.Fail();

    static void test<A>(
      Act<A> setupA = null,
      Option<ReferencesInPrefabs.ErrorType> errorType = new Option<ReferencesInPrefabs.ErrorType>()
    ) where A : Component {
      var go = new GameObject();
      var a = go.AddComponent<A>();
      setupA?.Invoke(a);
      var errors = ReferencesInPrefabs.findMissingReferences("", new [] { go }, false);
      errorType.voidFold(
        () => errors.shouldBeEmpty(),
        type => errors.shouldMatch(t => t.Exists(x => x.errorType == type))
      );
    }
  }
}
