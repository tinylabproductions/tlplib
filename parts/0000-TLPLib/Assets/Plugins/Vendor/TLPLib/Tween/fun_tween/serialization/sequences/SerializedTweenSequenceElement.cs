using System.Collections.Generic;
using AdvancedInspector;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// Everything that can go into <see cref="SerializedTweenSequence"/>.
  /// </summary>
  public abstract class SerializedTweenSequenceElement : ComponentMonoBehaviour {
    [PublicAPI] public abstract float duration { get; }
    [PublicAPI] public abstract IEnumerable<TweenSequenceElement> elements { get; }
  }
}