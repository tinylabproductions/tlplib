﻿using System.Collections.Generic;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenTimeline"/> as a <see cref="ComponentMonoBehaviour"/>.
  /// </summary>
  [AddComponentMenu("")]
  public partial class TimelineReference : SerializedTweenTimelineElement {
#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
    [SerializeField, PublicAccessor, NotNull] FunTweenTimeline _timeline;
    // ReSharper restore FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local, NotNullMemberIsNotInitialized
#pragma warning restore 649

    IEnumerable<TweenTimelineElement> _elements;

    public override float duration => _timeline.timeline.timeline.duration;
    public override IEnumerable<TweenTimelineElement> elements => 
      _elements ?? (_elements = _timeline.timeline.timeline.Yield<TweenTimelineElement>());

    public override void invalidate() {
      _elements = null;
      _timeline.invalidate();
    }
  }
}