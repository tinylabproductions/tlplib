using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  public abstract class SerializedTweenCallback : SerializedTweenSequenceElement {
    [PublicAPI] public abstract TweenCallback callback { get; }

    public override IEnumerable<TweenSequenceElement> elements {
      get { yield return callback; }
    }
  }
}