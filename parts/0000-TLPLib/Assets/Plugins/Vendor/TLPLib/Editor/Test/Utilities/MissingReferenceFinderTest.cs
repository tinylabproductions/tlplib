using System;
using System.Collections.Generic;
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
      public void setField (GameObject go) { field = go; }
    }

    class NonSerializedField : MonoBehaviour {
      [NotNull, NonSerialized] GameObject field;
      public void setField (GameObject go) { field = go; }

    }

    class ArrayWithNulls : MonoBehaviour {
      public GameObject[] field;
    }

    class NotNullArray : MonoBehaviour {
      [NotNull] public GameObject[] field;
    }

    class NullReferenceList : MonoBehaviour {
      [NotNull] public List<InnerNotNull> field;
    }

    [Serializable]
    public struct InnerNotNull {
      [NotNull] public GameObject field;  
    }

    class NullReferencePublicField : MonoBehaviour {
      public InnerNotNull field;
    }

    class NullReferenceSerializedField : MonoBehaviour {
      [SerializeField] InnerNotNull field;
      public void setField (InnerNotNull inn) { field = inn; }
    }

    #region Missing References
    [Test] public void WhenMissingReference() => test<TestClass>(
      a => {
        a.field = new GameObject();
        Object.DestroyImmediate(a.field);
      },
      MissingReferenceFinder.ErrorType.MISSING_REF.some()
    );
    [Test] public void WhenReferenceNotMissing() => test<TestClass>(
      a => {
        a.field = new GameObject();
      }
    );
    [Test] public void WhenMissingReferenceInner() => test<NullReferencePublicField>(
      a => {
        a.field.field = new GameObject();
        Object.DestroyImmediate(a.field.field);
      },
      MissingReferenceFinder.ErrorType.MISSING_REF.some()
    );
    [Test] public void WhenReferenceNotMissingInner() => test<NullReferencePublicField>(
      a => {
        a.field.field = new GameObject();
      }
    );
    #endregion

    #region Public/Serialized Field
    [Test] public void WhenNotNullPublicField() => test<NotNullPublicField>(
      errorType: MissingReferenceFinder.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNotNullPublicFieldSet() => test<NotNullPublicField>(
      a => {
        a.field = new GameObject();
      }
    );
    [Test] public void WhenNotNullSerializedField() => test<NotNullSerializedField>(
      errorType: MissingReferenceFinder.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNotNullSerializedFieldSet() => test<NotNullSerializedField>(
      a => {
        a.setField(new GameObject());
      }
    );
    #endregion

    #region Array/List
    [Test] public void WhenArrayWithNulls() => test<ArrayWithNulls>(
      a => {
        a.field = new [] { new GameObject(), null, new GameObject() };
      }
    );
    [Test] public void WhenNotNullArray() => test<NotNullArray>(
      a => {
        a.field = new [] { new GameObject(), null, new GameObject() };
      },
      MissingReferenceFinder.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNullReferenceListEmpty() => test<NullReferenceList>(
      a => {
        a.field = new List<InnerNotNull>();
      }
    );
    [Test] public void WhenNullReferenceList() => test<NullReferenceList>(
      a => {
        a.field = new List<InnerNotNull> { new InnerNotNull() };
      },
      MissingReferenceFinder.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNullReferenceListSet() => test<NullReferenceList>(
      a => {
        var inner = new InnerNotNull { field = new GameObject() };
        a.field = new List<InnerNotNull> { inner };
      }
    );
    #endregion

    [Test] public void WhenNonSerializedFieldIsNotSet() => test<NonSerializedField>();
    [Test] public void WhenNonSerializedFieldIsSet() => test<NonSerializedField>(
      a => {
        a.setField(new GameObject());
      }
    );

    [Test] public void WhenNullInsideMonoBehaviorPublicField() => test<NullReferencePublicField>(
      errorType: MissingReferenceFinder.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNullInsideMonoBehaviorPublicFieldSet() => test<NullReferencePublicField>(
      a => {
        a.field = new InnerNotNull {field = new GameObject()};
      }
    );
    [Test] public void WhenNullInsideMonoBehaviorSerializedField() => test<NullReferenceSerializedField>(
      errorType: MissingReferenceFinder.ErrorType.NULL_REF.some()
    );
    [Test] public void WhenNullInsideMonoBehaviorSerializedFieldSet() => test<NullReferenceSerializedField>(
      a => {
        a.setField(new InnerNotNull {field = new GameObject()});
      }
    );

    static void test<A>(
      Act<A> setupA = null,
      Option<MissingReferenceFinder.ErrorType> errorType = new Option<MissingReferenceFinder.ErrorType>()
    ) where A : Component {
      var go = new GameObject();
      var a = go.AddComponent<A>();
      setupA?.Invoke(a);
      var errors = MissingReferenceFinder.findMissingReferences("", new Object[] { go });
      errorType.voidFold(
        () => errors.shouldBeEmpty(),
        type => errors.shouldMatch(t => t.Exists(x => x.errorType == type))
      );
    }
  }
}
