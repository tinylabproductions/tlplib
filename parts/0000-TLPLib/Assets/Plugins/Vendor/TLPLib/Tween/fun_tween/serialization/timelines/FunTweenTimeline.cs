using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenTimeline"/> as a <see cref="MonoBehaviour"/>.
  /// </summary>
  public partial class FunTweenTimeline : MonoBehaviour, Invalidatable {
    [SerializeField, PublicAccessor, NotNull] SerializedTweenTimeline _timeline;
    
    public void invalidate() => _timeline.invalidate();
  }
}