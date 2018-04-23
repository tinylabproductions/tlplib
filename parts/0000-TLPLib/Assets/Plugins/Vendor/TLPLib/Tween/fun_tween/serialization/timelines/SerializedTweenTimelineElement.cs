using System.Collections.Generic;
using AdvancedInspector;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// Everything that can go into <see cref="SerializedTweenTimeline"/>.
  /// </summary>
  public abstract class SerializedTweenTimelineElement : ComponentMonoBehaviour {
    [PublicAPI] public abstract float duration { get; }
    [PublicAPI] public abstract IEnumerable<TweenTimelineElement> elements { get; }
  }
}