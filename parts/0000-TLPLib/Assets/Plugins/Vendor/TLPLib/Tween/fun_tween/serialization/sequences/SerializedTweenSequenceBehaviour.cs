using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenSequence"/> as a <see cref="MonoBehaviour"/>.
  /// </summary>
  public partial class SerializedTweenSequenceBehaviour : MonoBehaviour {
    [SerializeField, PublicAccessor, NotNull] SerializedTweenSequence _sequence;
  }
}