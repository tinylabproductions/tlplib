using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  /// <summary>
  /// <see cref="TweenTimeline"/> as a <see cref="MonoBehaviour"/>.
  /// </summary>
  public partial class FunTweenTimeline : MonoBehaviour, Invalidatable {

    #region Unity Serialized Fields
#pragma warning disable 649
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [SerializeField, PublicAccessor, NotNull] SerializedTweenTimeline _timeline;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649
    #endregion

    public void invalidate() => _timeline.invalidate();
  }
}