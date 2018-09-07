#if ADV_INS_CHANGES && UNITY_EDITOR
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using UnityEngine;
using Element = com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences.SerializedTweenTimeline.Element;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public class TimelineNode {
    public Element element { get; set; }
    public float startTime { get; set; }
    public float duration { get; set; }
    public int channel { get; set; }
    public string name { get; set; }
    public bool isCallback { get; set; }
    public Color nodeTextColor { get; private set; }

    public Option<TimelineNode> linkedNode { get; private set; }

    public float getEnd() => startTime + duration;
    
    public void link(TimelineNode linkTo) { linkedNode = linkTo.some(); }

    public void reLink(TimelineNode linkTo) {
      if (element.startAt == Element.At.AfterLastElement) {
        link(linkTo);
      }
    }
    
    public TimelineNode unlink() {
      linkedNode = F.none_;
      convert(Element.At.SpecificTime);
      return this;
    }

    public void refreshColor() =>
      nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;

    void setTimeOffset(float time) { element.timeOffset = time; }

    public TimelineNode(Element element,  float startTime, string name) {
      if (element.element && element.element is SerializedTweenCallback) {
        isCallback = true;
        duration = 0;
      }
      else {
        isCallback = false;
        duration = element.element ? element.element.duration : 10;
      }
      this.element = element;
      channel = element.timelineChannelIdx;
      this.startTime = startTime;
      this.name = name;
      nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;
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
            if (linkedNode.valueOut(out var linked)) {
              setTimeOffset(startTime - linked.getEnd());
              element.startAt = newStartType;
              element.element.setDuration(duration);
            }

            break;
          }
          case Element.At.SpecificTime: {
            setTimeOffset(startTime);
            element.startAt = newStartType;
            element.element.setDuration(duration);
            
            break;
          }
          case Element.At.WithLastElement: {
            if (linkedNode.valueOut(out var linked)) {
              setTimeOffset(startTime - linked.startTime);
              element.startAt = newStartType;
              element.element.setDuration(duration);
            }

            break;
          }
        }
      }
    }
    
  }
}
#endif
