using System.ComponentModel;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.unity_serialization {
  // Base class for property drawer.
  public abstract class UnityEither {
    [PublicAPI] public abstract bool isA { get; }
    [PublicAPI] public abstract bool isB { get; }

    public virtual string aDescription { get; } = "A";
    public virtual string bDescription { get; } = "B";
  }
  public abstract class UnityEither<A, B> : UnityEither, ISkipObjectValidationFields {
#pragma warning disable 649
    // protected is only needed for tests
    [SerializeField] bool _isA;
    [SerializeField, NotNull] A a;
    [SerializeField, NotNull] B b;
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

    public override bool isA => _isA;
    public override bool isB => !_isA;
    
    public override string aDescription { get; } = typeof(A).Name;
    public override string bDescription { get; } = typeof(B).Name;

    public Either<A, B> value => isB.either(a, b);

    public string[] blacklistedFields() => 
      _isA
      ? new [] { nameof(b) }
      : new [] { nameof(a) };
  }
}