using UnityEngine;

#if UNITY_EDITOR

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager {
  public partial class FunTweenManagerV2 {
    
  }

  public partial class SerializedTweenTimelineV2 {
    public partial class Element {
      public At startAt {
        get => _at;
        set => _at = value;
      }
      
      public float timeOffset {
        get => _timeOffset;
        set => _timeOffset = value;
      }

      string _title;
      public string title => _title ??= generateTitle();

      string generateTitle() {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (_element == null) return "NULL";
        var target = _element.getTarget();
        if (target is Component c) target = c.gameObject;
        return _element.GetType().Name + " : " + (target ? target.name : "NULL");
      }

      public int timelineChannelIdx { get; set; }

      public void invalidate() {
        _title = null;
      }
    }
    
    public Element[] elements {
      get => _elements;
      set => _elements = value;
    }
  }
}

#endif