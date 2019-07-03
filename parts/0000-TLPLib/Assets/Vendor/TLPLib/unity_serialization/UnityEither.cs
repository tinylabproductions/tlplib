using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  public abstract class UnityEither<A, B> : ISkipObjectValidationFields {
#pragma warning disable 649
    // protected is only needed for tests
    [SerializeField, ShowIf(nameof(validate)), LabelText("$" + nameof(isADescription))] bool _isA;
    [SerializeField, NotNull, ShowIf(nameof(isA)), LabelText("$" + nameof(aDescription))] A a;
    [SerializeField, NotNull, ShowIf(nameof(isB)), LabelText("$" + nameof(bDescription))] B b;
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

    protected virtual string isADescription { get; } = $"Is {typeof(A).Name}";
    protected virtual string aDescription { get; } = typeof(A).Name;
    protected virtual string bDescription { get; } = typeof(B).Name;

    public Either<A, B> value => isB.either(a, b);

    public string[] blacklistedFields() => 
      _isA
      ? new [] { nameof(b) }
      : new [] { nameof(a) };
  }
}