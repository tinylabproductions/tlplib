using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenTimeline"/> as a <see cref="MonoBehaviour"/>.
  /// </summary>
  public partial class SerializedTweenTimelineBehaviour : MonoBehaviour {
    [SerializeField, PublicAccessor, NotNull] SerializedTweenTimeline _timeline;
  }
}