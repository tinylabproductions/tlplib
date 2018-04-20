using System.Collections.Generic;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenSequence"/> as a <see cref="ComponentMonoBehaviour"/>.
  /// </summary>
  [AddComponentMenu("")]
  public partial class SerializedTweenSequenceComponentBehaviour : SerializedTweenSequenceElement {
    [SerializeField, PublicAccessor, NotNull] SerializedTweenSequenceBehaviour _sequence;

    IEnumerable<TweenSequenceElement> _elements;

    public override float duration => _sequence.sequence.sequence.duration;
    public override IEnumerable<TweenSequenceElement> elements => 
      _elements ?? (_elements = _sequence.sequence.sequence.Yield<TweenSequenceElement>());
  }
}