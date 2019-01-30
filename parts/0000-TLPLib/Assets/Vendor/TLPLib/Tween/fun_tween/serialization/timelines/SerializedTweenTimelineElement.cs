using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// Everything that can go into <see cref="SerializedTweenTimeline"/>.
  /// </summary>
  public abstract class SerializedTweenTimelineElement : TLPComponentMonoBehaviour, Invalidatable {
    [PublicAPI] public abstract float duration { get; }
    [PublicAPI] public abstract IEnumerable<TweenTimelineElement> elements { get; }
    public abstract void invalidate();
  }
}