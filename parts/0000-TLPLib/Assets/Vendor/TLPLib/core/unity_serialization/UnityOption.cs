using System;
using com.tinylabproductions.TLPLib.Components;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using pzd.lib.functional;
using pzd.lib.utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  // Base class for property drawer.
  public abstract class UnityOption {
    [PublicAPI] public abstract bool isSome { get; }
    [PublicAPI] public bool isNone => !isSome;
  }
  
  /// You need to extend this class and mark it as <see cref="SerializableAttribute"/>
  /// to serialize it, because Unity does not serialize generic classes.
  public abstract class UnityOption<A> : UnityOption, ISkipObjectValidationFields, Ref<Option<A>> {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [
      SerializeField, FormerlySerializedAs("isSome"),
      // Inspect, Descriptor(nameof(isSomeDescription))
    ] bool _isSome;
    [SerializeField, NotNull/*, Inspect(nameof(inspectValue)), Descriptor(nameof(description))*/] A _value;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649

    #endregion

    protected UnityOption() {}

    protected UnityOption(Option<A> value) {
      _isSome = value.isSome;
      foreach (var v in value) _value = v;
    }

    public override bool isSome { get {
      if (_isSome) {
        if (
#if UNITY_EDITOR
          Application.isPlaying &&
#endif
          !typeof(A).IsValueType && _value == null
        ) {
          Log.d.error(
            $"{nameof(UnityOption<A>)} of {GetType()} was marked as Some, but referencing value was null!"
          );
          return false;
        }

        return true;
      }
      return false;
    } }

    public A __unsafeGet => _value;

    public bool valueOut(out A a) {
      a = _value;
      return _isSome;
    }

    bool inspectValue() {
      // ReSharper disable once AssignNullToNotNullAttribute
      if (!_isSome) _value = default;
      return _isSome;
    }

    // protected virtual Description isSomeDescription { get; } = new Description("Set?");
    // protected virtual Description description { get; } = new Description("Value");

    public static implicit operator Option<A>(UnityOption<A> o) => o.value;
    public Option<A> value => isSome ? F.some(_value) : None._;
    Option<A> Ref<Option<A>>.value {
      get { return value; }
      set {
        _isSome = value.isSome;
        // ReSharper disable once AssignNullToNotNullAttribute
        _value = value.isSome ? value.__unsafeGet : default(A);
      }
    }


    public static bool operator ==(UnityOption<A> left, UnityOption<A> right) => left.value == right.value;

    public static bool operator !=(UnityOption<A> left, UnityOption<A> right) => left.value != right.value;

    public string[] blacklistedFields() =>
      isSome
      ? new string[] {}
      : new [] { nameof(_value) };

    public override string ToString() => value.ToString();
  }

  [Serializable, PublicAPI]
  public class UnityOptionInt : UnityOption<int> {
    public UnityOptionInt() { }
    public UnityOptionInt(Option<int> value) : base(value) { }
  }
  [Serializable, PublicAPI] public class UnityOptionFloat : UnityOption<float> {}
  [Serializable, PublicAPI] public class UnityOptionBool : UnityOption<bool> {}
  [Serializable, PublicAPI]
  public class UnityOptionString : UnityOption<string> {
    public UnityOptionString() { }
    public UnityOptionString(Option<string> value) : base(value) { }
  }
  [Serializable, PublicAPI] public class UnityOptionByteArray : UnityOption<byte[]> {
    public UnityOptionByteArray(Option<byte[]> value) : base(value) { }
  }
  [Serializable, PublicAPI] public class UnityOptionVector2 : UnityOption<Vector2> {}
  [Serializable, PublicAPI] public class UnityOptionVector3 : UnityOption<Vector3> {}
  [Serializable, PublicAPI] public class UnityOptionVector4 : UnityOption<Vector4> {}
  [Serializable, PublicAPI] public class UnityOptionColor : UnityOption<Color> {}
  [Serializable, PublicAPI] public class UnityOptionMonoBehaviour : UnityOption<MonoBehaviour> {}
  [Serializable, PublicAPI] public class UnityOptionMonoBehaviours : UnityOption<MonoBehaviour[]> {}
  [Serializable, PublicAPI] public class UnityOptionGraphicStyle : UnityOption<GraphicStyle> {}
  [Serializable, PublicAPI] public class UnityOptionAudioClip : UnityOption<AudioClip> {}
  [Serializable, PublicAPI] public class UnityOptionUShort : UnityOption<ushort> {
    public UnityOptionUShort() { }
    public UnityOptionUShort(Option<ushort> value) : base(value) { }
  }
  [Serializable, PublicAPI] public class UnityOptionUInt : UnityOption<uint> {
    public UnityOptionUInt() { }
    public UnityOptionUInt(Option<uint> value) : base(value) { }
  }
  [Serializable, PublicAPI] public class UnityOptionUIntArray : UnityOption<uint[]> { }
  [Serializable, PublicAPI]
  public class UnityOptionULong : UnityOption<ulong> {
    public UnityOptionULong() { }
    public UnityOptionULong(Option<ulong> value) : base(value) { }
  }
  [Serializable, PublicAPI] public class UnityOptionULongArray : UnityOption<ulong[]> { }
  [Serializable, PublicAPI] public class UnityOptionGameObject : UnityOption<GameObject> {
    public UnityOptionGameObject() {}
    public UnityOptionGameObject(Option<GameObject> value) : base(value) {}
  }
  [Serializable, PublicAPI] public class UnityOptionGameObjects : UnityOption<GameObject[]> {}
  [Serializable, PublicAPI] public class UnityOptionComponent : UnityOption<Component> {}
  [Serializable, PublicAPI] public class UnityOptionComponents : UnityOption<Component[]> {}
  [Serializable, PublicAPI] public class UnityOptionRigidbody2D : UnityOption<Rigidbody2D> { }
  [Serializable, PublicAPI] public class UnityOptionText : UnityOption<Text> {}
  [Serializable, PublicAPI] public class UnityOptionUIClickForwarder : UnityOption<UIClickForwarder> { }
  [Serializable, PublicAPI] public class UnityOptionTransform : UnityOption<Transform> { }
  [Serializable, PublicAPI] public class UnityOptionImage : UnityOption<Image> { }
  [Serializable, PublicAPI] public class UnityOptionTexture2D : UnityOption<Texture2D> {}
  [Serializable, PublicAPI] public class UnityOptionKeyCode : UnityOption<KeyCode> {}
  [Serializable, PublicAPI] public class UnityOptionDuration: UnityOption<Duration> {}
  [Serializable, PublicAPI] public class UnityOptionSprite : UnityOption<Sprite> {}
  [Serializable, PublicAPI] public class UnityOptionParticleSystem : UnityOption<ParticleSystem> {}
  [Serializable, PublicAPI] public class UnityOptionTrailRenderer : UnityOption<TrailRenderer> {}
  [Serializable, PublicAPI] public class UnityOptionLayerMask : UnityOption<LayerMask> {}
}
