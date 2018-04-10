using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using com.tinylabproductions.TLPLib.validations;
using GenerationAttributes;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ErrorType = com.tinylabproductions.TLPLib.Utilities.Editor.ObjectValidator.Error.Type;
// ReSharper disable MemberCanBePrivate.Local

// ReSharper disable ClassNeverInstantiated.Local, NotNullMemberIsNotInitialized, NotAccessedField.Local
#pragma warning disable 169

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public partial class ObjectValidatorTest : ImplicitSpecification {
    public const string UNIQUE_CATEGORY = "UNIQUE_CATEGORY";

    class Component1 : MonoBehaviour { }
    class Component2 : MonoBehaviour { }
    class Component3 : MonoBehaviour { }
    class Component3Child : Component3 { }

    #region Test Classes

    class PublicField : MonoBehaviour {
      public GameObject field;
    }

    class NotNullPublicFieldObject : MonoBehaviour {
      [NotNull] public Object field;
    }

    class NotNullPublicField : MonoBehaviour {
      [NotNull] public GameObject field;
    }

    class NotNullSerializedField : MonoBehaviour {
      [NotNull, SerializeField] GameObject field;
      public void setField (GameObject go) { field = go; }
    }

    class PublicFieldExtended : PublicField {
#pragma warning disable 649, 414
      [NotNull, SerializeField] GameObject field2;
#pragma warning restore 649, 414
    }

    class NotNullPublicFieldExtended : NotNullPublicField { }

    class NotNullSerializedFieldExtended : NotNullSerializedField { }

    class NonSerializedField : MonoBehaviour {
      [NotNull, NonSerialized] GameObject field;
      public void setField (GameObject go) { field = go; }
    }

    class ArrayWithNulls : MonoBehaviour {
      public GameObject[] field;
    }

    class ListNotEmpty : MonoBehaviour {
      [NonEmpty] public List<InnerNotNull> field;
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

    class NotNullProtectedSerializedField : MonoBehaviour {
      [NotNull, SerializeField] protected GameObject field;
      public void setField (GameObject go) { field = go; }
    }

    class NullReferenceProtectedSerializedField : MonoBehaviour {
      [SerializeField] protected InnerNotNull field;
      public void setField (InnerNotNull inn) { field = inn; }
    }

    [RequireComponent(typeof(Component1), typeof(Component2), typeof(Component3))]
    class RequireComponentBehaviour : MonoBehaviour {
      public void setup(bool first = true, bool second = true, bool third = true) {
        var go = gameObject;
        if (first) go.AddComponent<Component1>();
        if (second) go.AddComponent<Component2>();
        // Inheriting from Component3
        if (third) go.AddComponent<Component3Child>();
      }
    }

    class InheritingRequireComponentBehaviour : RequireComponentBehaviour {}

    class TextFieldTypeNotTag : MonoBehaviour {
#pragma warning disable 649
      [TextField(TextFieldType.Area)]
      public string field;
#pragma warning restore 649
    }

    class TextFieldTypeTag : MonoBehaviour {
      [TextField(TextFieldType.Tag)]
      public string field;
    }

    class OnObjectValidateThrowException : MonoBehaviour, OnObjectValidate {
      public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) { throw new Exception("I'm broken"); }
    }

    class OnObjectValidateThrowLazyException : MonoBehaviour, OnObjectValidate {
      public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
        yield return new ErrorMsg("test1");
        throw new Exception("I'm broken");
      }
    }

    class OnObjectValidateReturnsNull : MonoBehaviour, OnObjectValidate {
      public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) => null;
    }

    #endregion

    #region UniqueValue duplicates

    [Serializable, Record]
    public partial struct UniqueValueStruct {
      [UniqueValue(UNIQUE_CATEGORY)] readonly byte[] identifier;
    }

    [Test]
    public void uniqueValuesComparer() {
      describe(() => {
        void test<A>(Fn<A> createValue1, Fn<A> createValue2) =>
          when[typeof(A).Name] = () => {
            it["should work on same values"] = () => ObjectValidator.UniqueValuesCache.comparer.Equals(
              createValue1(), createValue1()
            ).shouldBeTrue();

            it["should work on different values"] = () => ObjectValidator.UniqueValuesCache.comparer.Equals(
              createValue1(), createValue2()
            ).shouldBeFalse();
          };

        test(() => new byte[] {0, 1}, () => new byte[] {0, 2});
        test(() => new[] {"0", "1"}, () => new [] {"0", "2"});
        test(() => "foo", () => "bar");
        test(() => 0, () => 1);
        test(() => 0u, () => 1u);
        test(() => 0L, () => 1L);
      });
    }

    [Test]
    public void uniqueValuesInScriptableObject() {
      describe(() => {
        when["duplicates exist"] = () => {
          it["should fail"] = () => {
            errorsInScriptableObject<UniqueValueScriptableObject>(
              a => {
                a.identifier = new byte[] {1, 2};
                a.identifier2 = new byte[] {1, 2};
              }
            ).shouldHave(ErrorType.DuplicateUniqueValue);
          };
        };
        when["there are no duplicates"] = () => {
          it["should pass"] = () =>
            errorsInScriptableObject<UniqueValueScriptableObject>(
              a => {
                a.identifier = new byte[] {1, 2};
                a.identifier2 = new byte[] {1, 3};
              }
            ).shouldBeEmpty();
        };
      });
    }

    [Test]
    public void uniqueValuesInScriptableObjectLists() {
      describe(() => {
        when["duplicates exist"] = () => {
          it["should fail"] = () => {
            errorsInScriptableObject<UniqueValueScriptableObjectWithList>(
              _ => _.listOfStructs = new List<UniqueValueStruct> {
                new UniqueValueStruct(new byte[] {1, 2}),
                new UniqueValueStruct(new byte[] {1, 2})
              }
            ).shouldHave(ErrorType.DuplicateUniqueValue);
          };
        };
        when["there are no duplicates"] = () => {
          it["should pass"] = () => {
            errorsInScriptableObject<UniqueValueScriptableObjectWithList>(
              _ => _.listOfStructs = new List<UniqueValueStruct> {
                new UniqueValueStruct(new byte[] {1, 2}),
                new UniqueValueStruct(new byte[] {1, 3})
              }
            ).shouldBeEmpty();
          };
        };
      });
    }

    #endregion

    [Test]
    public void customObjectValidator() => describe(() => {
      when["method throws exception"] = () => {
        it["should catch it"] = () => shouldFindErrors<OnObjectValidateThrowException>(
          ErrorType.CustomValidationException
        );
      };
      when["method return null"] = () => {
        it["should catch it"] = () => shouldFindErrors<OnObjectValidateReturnsNull>(
          ErrorType.CustomValidationException
        );
      };
      when["method throws exception in lazy evaluation"] = () => {
        it["should catch it"] = () => shouldFindErrors<OnObjectValidateThrowLazyException>(
          ErrorType.CustomValidationException
        );
      };
    });

    #region Missing References

    static readonly LazyVal<PathStr> testPrefabsDirectory =
      new LazyValImpl<PathStr>(() =>
        // There is no such thing as editor resources folder, so we have to resort to this hack
        AssetDatabase.GetAllAssetPaths().find(s =>
          s.EndsWithFast($"TLPLib/Editor/Test/Utilities/{nameof(ObjectValidatorTest)}.cs")
        ).map(p => PathStr.a(p).dirname).get
      );

    static Object getPrefab(string prefabName) =>
      AssetDatabase.LoadMainAssetAtPath($"{testPrefabsDirectory.strict}/{prefabName}");

    [Test] public void WhenMissingComponent() {
      var go = getPrefab("TestMissingComponent.prefab");
      var errors = ObjectValidator.check(ObjectValidator.CheckContext.empty, new [] { go });
      errors.shouldHave(ErrorType.MissingComponent);
    }

    [Test] public void WhenMissingReference() =>
      shouldFindErrors<PublicField>(
        ErrorType.MissingReference,
        a => {
          a.field = new GameObject();
          Object.DestroyImmediate(a.field);
        }
      );

    [Test] public void WhenReferenceNotMissing() =>
      shouldNotFindErrors<PublicField>(a => {
        a.field = new GameObject();
      });

    [Test] public void WhenMissingReferenceInner() =>
      shouldFindErrors<NullReferencePublicField>(
        ErrorType.MissingReference,
        a => {
          a.field.field = new GameObject();
          Object.DestroyImmediate(a.field.field);
        }
      );

    [Test] public void WhenReferenceNotMissingInner() =>
      shouldNotFindErrors<NullReferencePublicField>(a => {
        a.field.field = new GameObject();
      });

    #endregion

    #region Public/Serialized Field

    [Test] public void WhenNotNullPublicField()
      => shouldFindErrors<NotNullPublicField>(ErrorType.NullReference);

    [Test] public void WhenNotNullPublicFieldSet() =>
      shouldNotFindErrors<NotNullPublicField>(a => {
        a.field = new GameObject();
      });

    [Test] public void WhenNotNullSerializedField()
      => shouldFindErrors<NotNullSerializedField>(ErrorType.NullReference);

    [Test] public void WhenPublicFieldExtended()
      => shouldFindErrors<PublicFieldExtended>(ErrorType.NullReference);

    [Test] public void WhenNotNullPublicFieldExtended()
      => shouldFindErrors<NotNullPublicFieldExtended>(ErrorType.NullReference);

    [Test] public void WhenNotNullSerializedFieldExtended()
      => shouldFindErrors<NotNullSerializedFieldExtended>(ErrorType.NullReference);

    [Test] public void WhenNotNullPublicFieldObjectSet() =>
      shouldFindErrors<NotNullPublicFieldObject>(
        ErrorType.NullReference,
        a => {
          a.field = new Object();
        }
      );

    [Test] public void WhenNotNullSerializedFieldSet() =>
      shouldNotFindErrors<NotNullSerializedField>(a => {
        a.setField(new GameObject());
      });

    #endregion

    #region Array/List

    [Test] public void WhenArrayWithNulls() =>
      shouldNotFindErrors<ArrayWithNulls>(
        a => { a.field = new[] {new GameObject(), null, new GameObject()}; }
      );

    [Test] public void WhenNotNullArray() =>
      shouldFindErrors<NotNullArray>(
        ErrorType.NullReference,
        a => { a.field = new[] {new GameObject(), null, new GameObject()}; }
      );

    [Test] public void WhenReferenceListEmpty() =>
      shouldFindErrors<ListNotEmpty>(
        ErrorType.EmptyCollection,
        a => {
          a.field = new List<InnerNotNull>();
        }
      );

    [Test] public void WhenReferenceListNotEmpty() =>
      shouldNotFindErrors<ListNotEmpty>(
        a => {
          var inner = new InnerNotNull { field = new GameObject() };
          a.field = new List<InnerNotNull> { inner };
        }
      );

    [Test] public void WhenNullReferenceList() =>
      shouldFindErrors<NullReferenceList>(
        ErrorType.NullReference,
        a => {
          a.field = new List<InnerNotNull> { new InnerNotNull() };
        }
      );

    [Test] public void WhenNullReferenceListSet() =>
      shouldNotFindErrors<NullReferenceList>(
        a => {
          var inner = new InnerNotNull {field = new GameObject()};
          a.field = new List<InnerNotNull> {inner};
        }
      );

    #endregion

    [Test] public void WhenNonSerializedFieldIsNotSet() => shouldNotFindErrors<NonSerializedField>();

    [Test] public void WhenNonSerializedFieldIsSet() =>
      shouldNotFindErrors<NonSerializedField>(a => {
        a.setField(new GameObject());
      });

    [Test] public void WhenNullInsideMonoBehaviorPublicField() =>
      shouldFindErrors<NullReferencePublicField>(
        errorType: ErrorType.NullReference
      );

    [Test] public void WhenNullInsideMonoBehaviorPublicFieldSet() =>
      shouldNotFindErrors<NullReferencePublicField>(a => {
        a.field = new InnerNotNull {field = new GameObject()};
      });

    [Test] public void WhenNullInsideMonoBehaviorSerializedField() =>
      shouldFindErrors<NullReferenceSerializedField>(
        ErrorType.NullReference
      );

    [Test] public void WhenNullInsideMonoBehaviorSerializedFieldSet() =>
      shouldNotFindErrors<NullReferenceSerializedField>(a => {
        a.setField(new InnerNotNull {field = new GameObject()});
      });

    [Test] public void WhenNotNullProtectedSerializedField()
      => shouldFindErrors<NotNullProtectedSerializedField>(ErrorType.NullReference);

    [Test] public void WhenNotNullProtectedSerializedFieldSet() =>
      shouldNotFindErrors<NotNullProtectedSerializedField>(a => {
        a.setField(new GameObject());
      });

    [Test] public void WhenNullInsideMonoBehaviorProtectedSerializedField() =>
      shouldFindErrors<NullReferenceProtectedSerializedField>(
        ErrorType.NullReference
      );

    [Test] public void WhenNullInsideMonoBehaviorProtectedSerializedFieldSet() =>
      shouldNotFindErrors<NullReferenceProtectedSerializedField>(a => {
        a.setField(new InnerNotNull {field = new GameObject()});
      });

    #region [TextField(TextFieldType.Tag)]

    [Test] public void WhenTextFieldTypeNotTag() => shouldNotFindErrors<TextFieldTypeNotTag>();

    [Test] public void WhenBadTextFieldValue() =>
      shouldFindErrors<TextFieldTypeTag>(
        ErrorType.TextFieldBadTag,
        a => { a.field = ""; }
      );

    [Test] public void WhenGoodTextFieldValue() =>
      shouldNotFindErrors<TextFieldTypeTag>(a => {
        a.field = UnityEditorInternal.InternalEditorUtility.tags.First();
      });

    #endregion

    #region RequireComponent

    [Test] public void WhenRequireComponentComponentsAreThere() =>
       shouldNotFindErrors<RequireComponentBehaviour>(a => a.setup());

    [Test] public void WhenRequireComponentFirstComponentIsNotThere() =>
      shouldFindErrors<RequireComponentBehaviour>(
        ErrorType.MissingRequiredComponent,
        a => a.setup(first: false)
      );

    [Test] public void WhenRequireComponentSecondComponentIsNotThere() =>
      shouldFindErrors<RequireComponentBehaviour>(
        ErrorType.MissingRequiredComponent,
        a => a.setup(second: false)
      );

    [Test] public void WhenRequireComponentThirdComponentIsNotThere() =>
      shouldFindErrors<RequireComponentBehaviour>(
        ErrorType.MissingRequiredComponent,
        a => a.setup(third: false)
      );

    [Test] public void WhenInheritingRequireComponentComponentsAreThere() =>
      shouldNotFindErrors<InheritingRequireComponentBehaviour>(a => a.setup());

    [Test] public void WhenInheritingRequireComponentFirstComponentIsNotThere() =>
      shouldFindErrors<InheritingRequireComponentBehaviour>(
        ErrorType.MissingRequiredComponent,
        a => a.setup(first: false)
      );

    [Test] public void WhenInheritingRequireComponentSecondComponentIsNotThere() =>
      shouldFindErrors<InheritingRequireComponentBehaviour>(
        ErrorType.MissingRequiredComponent,
        a => a.setup(second: false)
      );

    [Test] public void WhenInheritingRequireComponentThirdComponentIsNotThere() =>
      shouldFindErrors<InheritingRequireComponentBehaviour>(
        ErrorType.MissingRequiredComponent,
        a => a.setup(third: false)
      );

    #endregion

    #region UnityEvent

    static void testPrefab(string prefabName, ErrorType errorType) {
      var go = getPrefab(prefabName);
      var errors = ObjectValidator.check(ObjectValidator.CheckContext.empty, new[] { go });
      errors.shouldHave(errorType);
    }

    [Test] public void WhenUnityEventInvalid() =>
      testPrefab("TestUnityEventInvalid.asset", ErrorType.UnityEventInvalid);

    [Test] public void WhenUnityEventInvalidMethod() =>
      testPrefab("TestUnityEventInvalidMethod.asset", ErrorType.UnityEventInvalidMethod);

    [Test] public void WhenUnityEventInvalidNested() =>
      testPrefab("TestUnityEventInvalidNested.asset", ErrorType.UnityEventInvalid);

    [Test] public void WhenUnityEventInvalidNestedInArray() =>
      testPrefab("TestUnityEventInvalidNestedInArray.asset", ErrorType.UnityEventInvalid);

    [Test] public void WhenUnityEventGenericInvalid() =>
      testPrefab("TestUnityEventGeneric.asset", ErrorType.UnityEventInvalid);

    [Test] public void WhenUnityEventGenericInAssetInvalid() =>
      testPrefab("TestUnityEventGenericInArray.asset", ErrorType.UnityEventInvalid);

    #endregion

    static GameObject setupGOWithComponent<A>(Act<A> setupA = null) where A : Component {
      var go = new GameObject();
      var a = go.AddComponent<A>();
      setupA?.Invoke(a);
      return go;
    }
    static ScriptableObject setupScriptableObject<A>(Act<A> setup = null) where A : ScriptableObject {
      var so = ScriptableObject.CreateInstance<A>();
      setup?.Invoke(so);
      return so;
    }

    static ImmutableList<ObjectValidator.Error> checkForErrors<A>(
      Fn<A> create, Option<ObjectValidator.UniqueValuesCache> uniqueValuesCache = default
    ) where A : Object =>
      ObjectValidator.check(
        ObjectValidator.CheckContext.empty, new Object[] {create()},
        uniqueValuesCache: uniqueValuesCache
      );

    static ImmutableList<ObjectValidator.Error> errorsInScriptableObject<A>(
      Act<A> setup = null
    ) where A : ScriptableObject =>
      checkForErrors(() => setupScriptableObject(setup), ObjectValidator.UniqueValuesCache.create.some());

    public static void shouldNotFindErrors<A>(
      Act<A> setup = null
    ) where A : Component =>
      checkForErrors(() => setupGOWithComponent(setup)).shouldBeEmpty();

    public static void shouldFindErrors<A>(
      ErrorType errorType, Act<A> setup = null
    ) where A : Component =>
      checkForErrors(() => setupGOWithComponent(setup)).shouldHave(errorType);
  }

  public static class ErrorValidationExts {
    public static void shouldHave(this ImmutableList<ObjectValidator.Error> errors, ErrorType type) =>
      errors.shouldMatch(
        t => t.Exists(x => x.type == type),
        $"{type} does not exist in errors {errors.asDebugString()}"
      );
  }
}

