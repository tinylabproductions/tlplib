using System.Collections.Generic;
using AdvancedInspector;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  public abstract class SerializedTweenSequenceElement : ComponentMonoBehaviour {
    public abstract IEnumerable<TweenSequenceElement> elements { get; }
  }
}