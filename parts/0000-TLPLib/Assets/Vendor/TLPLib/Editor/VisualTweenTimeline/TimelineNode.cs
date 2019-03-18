#if ADV_INS_CHANGES && UNITY_EDITOR
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using GenerationAttributes;
using UnityEngine;
using Element = com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences.SerializedTweenTimeline.Element;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public partial class TimelineNode {
    [PublicAccessor] float _duration;
    [PublicAccessor] float _startTime;
    [PublicAccessor] int _channel;
    [PublicAccessor] Color _nodeTextColor;
    [PublicAccessor] Option<TimelineNode> _linkedNode;
    public readonly string name;
    public readonly bool isCallback;
    public readonly Element element;

    public float getEnd() => _startTime + _duration;
    
    public void linkTo(TimelineNode linkTo) {
      _linkedNode = linkTo.some();

      if (element.startAt != Element.At.AfterLastElement) {
        convert(Element.At.AfterLastElement);
      }
    }

    void setChannel(int idx) => _channel = Mathf.Clamp(idx, 0, int.MaxValue);
    public void increaseChannel() => setChannel(_channel + 1);
    public void decreaseChannel() => setChannel(_channel - 1);

    public void setDuration(float durationToSet) =>
      _duration = Mathf.Clamp(durationToSet, 0.01f, float.MaxValue);

    public void setStartTime(float timeToSet, float lowerBound = 0) {
      if (element.element != null) {
         _startTime = Mathf.Clamp(timeToSet, lowerBound, float.MaxValue);
      }
    }
    
    public void unlink() {
      _linkedNode = F.none_;
      convert(Element.At.SpecificTime);
    }

    public void refreshColor() =>
      _nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;

    public void setTimeOffset(float time) { element.timeOffset = time; }

    public TimelineNode(Element element,  float startTime, string name) {
      if (element.element && element.element is SerializedTweenCallback) {
        isCallback = true;
        _duration = 0;
      }
      else {
        isCallback = false;
        _duration = element.element ? element.element.duration : 10;
      }
      this.name = name;
      this.element = element;
      _channel = element.timelineChannelIdx;
      _startTime = startTime;
      _nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;
    }

    static Color elementToColor(SerializedTweenTimelineElement element) {
      switch ((object) element) {
        case Transform_Position _:         return Color.yellow;
        case Path_Transfrom_Position _:    return Color.cyan;
        case Transform_LocalEulerAngles _: return new Color(1f, 0.75f, 1f);
        case Transform_Rotation _:         return Color.green;
        case Transform_LocalScale _:       return new Color(0.75f, 0.25f, 1);
        case Light_Color _:                return new Color(0.75f, 1, 1);
        case Light_Intensity _:            return new Color(1, 1, 0.75f);
        case Graphic_ColorAlpha _:         return new Color(0.25f, 0.75f, 1f);
        case Graphic_Color _:              return new Color(1f, 0.5f, 0f);
        default:                           return Color.white;
      }
    }
    
    public void convert(Element.At newStartType) {
      if (element.element != null) {
        switch (newStartType) {
          case Element.At.AfterLastElement: {
            if (_linkedNode.valueOut(out var linked)) {
              setTimeOffset(_startTime - linked.getEnd());
              element.startAt = newStartType;
              setDuration(_duration);
            }

            break;
          }
          case Element.At.SpecificTime: {
            setTimeOffset(_startTime);
            element.startAt = newStartType;
            setDuration(_duration);
            
            break;
          }
          case Element.At.WithLastElement: {
            if (_linkedNode.valueOut(out var linked)) {
              setTimeOffset(_startTime - linked._startTime);
              element.startAt = newStartType;
              setDuration(_duration);
            }

            break;
          }
        }
      }
    }
    
  }
}
#endif
