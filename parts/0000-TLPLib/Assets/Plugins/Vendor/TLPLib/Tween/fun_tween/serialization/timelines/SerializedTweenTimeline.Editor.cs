#if UNITY_EDITOR
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences {
  public partial class SerializedTweenTimeline {

    public Element[] elements {
      get => _elements;
      set => _elements = value;
    }
    
    public partial class Element {
      
      #region Unity Serialized Fields
#pragma warning disable 649
      // ReSharper disable FieldCanBeMadeReadOnly.Local
      [SerializeField, HideInInspector] int _timelineChannelIdx;
      // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 649
      #endregion
      
      public int timelineChannelIdx {
        get => _timelineChannelIdx;
        set => _timelineChannelIdx = value;
      }
      
      public string title {
        get => _title;
        set => _title = value;
      }
  
      public At startAt {
        get => _at;
        set => _at = value;
      }
  
      public float timeOffset {
        get => _timeOffset;
        set => _timeOffset = value;
      }
    }
  }
}
#endif