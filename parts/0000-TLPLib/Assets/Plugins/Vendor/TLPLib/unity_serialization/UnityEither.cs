using System.Diagnostics;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Components.sorting_layer;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using Plugins.Vendor.TLPLib.Editor.CustomEditors;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  [AdvancedInspector(false)]
  public abstract class UnityEither<A, B> : GenericUnityEitherParent, ISkipObjectValidationFields {
#pragma warning disable 649
    // protected is only needed for tests
    [SerializeField, 
//     Inspect(nameof(validate)), Descriptor(nameof(isADescription))
    ] bool _isA;
    [SerializeField,
//     Inspect(nameof(isA)), Descriptor(nameof(aDescription)),
     NotNull] A a;
    [SerializeField,
//     Inspect(nameof(isB)), Descriptor(nameof(bDescription)),
     NotNull] B b;
#pragma warning restore 649

    // ReSharper disable once NotNullMemberIsNotInitialized
    protected UnityEither() {}

    // ReSharper disable once NotNullMemberIsNotInitialized
    protected UnityEither(Either<A, B> either) {
      _isA = either.isLeft;
      if (either.isLeft)
        a = either.__unsafeGetLeft;
      else
        b = either.__unsafeGetRight;
    }

    bool validate() {
      // ReSharper disable AssignNullToNotNullAttribute
      if (isA) b = default;
      else a = default;
      // ReSharper restore AssignNullToNotNullAttribute
      return true;
    }

    public bool isA => _isA;
    public bool isB => !_isA;

    protected virtual Description isADescription { get; } = new Description($"Is {typeof(A).Name}");
    protected virtual Description aDescription { get; } = new Description(typeof(A).Name);
    protected virtual Description bDescription { get; } = new Description(typeof(B).Name);

    public Either<A, B> value => isB.either(a, b);

    public string[] blacklistedFields() => 
      _isA
      ? new [] { nameof(b) }
      : new [] { nameof(a) };
  }
}