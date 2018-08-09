using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenTimeline"/> as a <see cref="MonoBehaviour"/>.
  /// </summary>
  public partial class FunTweenTimeline : MonoBehaviour, Invalidatable {
#pragma warning disable 649
    [SerializeField, PublicAccessor, NotNull] SerializedTweenTimeline _timeline;
#pragma warning restore 649
    public void invalidate() => _timeline.invalidate();
  }
}