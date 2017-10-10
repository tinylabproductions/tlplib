using System;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPGame.unity_serialization {
  public abstract class UnityOption<A> : ISkipObjectValidationFields, Ref<Option<A>> {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [
      SerializeField, Inspect, FormerlySerializedAs("isSome"),
      Descriptor(nameof(isSomeDescription))
    ] bool _isSome;
    [SerializeField, Inspect(nameof(inspectValue)), Descriptor(nameof(description)), NotNull] A _value;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    protected UnityOption() {}

    protected UnityOption(Option<A> value) {
      _isSome = value.isSome;
      foreach (var v in value) _value = v;
    }

    public bool isSome { get {
      if (_isSome) {
        if (Application.isPlaying && _value == null) {
          if (Log.isError) Log.error(
            $"{nameof(UnityOption<A>)} of {GetType()} was marked as Some, but referencing value was null!"
          );
          return false;
        }

        return true;
      }
      return false;
    } }

    public bool isNone => !isSome;

    bool inspectValue() {
      // ReSharper disable once AssignNullToNotNullAttribute
      if (!_isSome) _value = default(A);
      return _isSome;
    }

    protected virtual Description isSomeDescription { get; } = new Description("Set?");
    protected virtual Description description { get; } = new Description("Value");

    public static implicit operator Option<A>(UnityOption<A> o) => o.value;
    public Option<A> value => isSome ? F.some(_value) : Option<A>.None;
    Option<A> Ref<Option<A>>.value {
      get { return value; }
      set {
        _isSome = value.isSome;
        // ReSharper disable once AssignNullToNotNullAttribute
        _value = value.isSome ? value.__unsafeGetValue : default(A);
      }
    }

    public string[] blacklistedFields() => 
      isSome
      ? new string[] {}
      : new [] { nameof(_value) };

    public override string ToString() => $"{nameof(UnityOption<A>)}({value})";
  }

  [Serializable] public class UnityOptionInt : UnityOption<int> {}
  [Serializable] public class UnityOptionFloat : UnityOption<float> {}
  [Serializable] public class UnityOptionBool : UnityOption<bool> {}
  [Serializable]
  public class UnityOptionString : UnityOption<string> {
    public UnityOptionString() { }
    public UnityOptionString(Option<string> value) : base(value) { }
  }
  [Serializable] public class UnityOptionVector2 : UnityOption<Vector2> {}
  [Serializable] public class UnityOptionVector3 : UnityOption<Vector3> {}
  [Serializable] public class UnityOptionVector4 : UnityOption<Vector4> {}
  [Serializable] public class UnityOptionColor : UnityOption<Color> {}
  [Serializable] public class UnityOptionMonoBehaviour : UnityOption<MonoBehaviour> {}
  [Serializable] public class UnityOptionGraphicStyle : UnityOption<GraphicStyle> {}
  [Serializable] public class UnityOptionAudioClip : UnityOption<AudioClip> {}
  [Serializable] public class UnityOptionUIntArray : UnityOption<uint[]> { }
  [Serializable] public class UnityOptionGameObject : UnityOption<GameObject> {
    public UnityOptionGameObject() {}
    public UnityOptionGameObject(Option<GameObject> value) : base(value) {}
  }
  [Serializable] public class UnityOptionRigidbody2D : UnityOption<Rigidbody2D> { }
  [Serializable] public class UnityOptionText : UnityOption<Text> {}
  [Serializable] public class UnityOptionUIClickForwarder : UnityOption<UIClickForwarder> { }
  [Serializable] public class UnityOptionTransform : UnityOption<Transform> { }
  [Serializable] public class UnityOptionImage : UnityOption<Image> { }
}