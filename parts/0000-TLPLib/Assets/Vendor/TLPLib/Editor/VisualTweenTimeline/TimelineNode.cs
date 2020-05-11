#if UNITY_EDITOR
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners;
using pzd.lib.exts;
using pzd.lib.functional;
using UnityEngine;
using Element = com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager.SerializedTweenTimelineV2.Element;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public class TimelineNode {
    public float duration { get; private set; }
    public float startTime { get; private set; }
    public int channel { get; private set; }
    public Color nodeTextColor { get; private set; }
    public Option<TimelineNode> linkedNode { get; private set; }
    
    public readonly bool isCallback;
    public readonly Element element;
    
    public string name => element.title;

    public float getEnd() => startTime + duration;
    
    public void linkTo(TimelineNode linkTo) {
      linkedNode = linkTo.some();

      if (element.startAt != Element.At.AfterLastElement) {
        convert(Element.At.AfterLastElement);
      }
    }

    void setChannel(int idx) => channel = Mathf.Clamp(idx, 0, int.MaxValue);
    public void increaseChannel() => setChannel(channel + 1);
    public void decreaseChannel() => setChannel(channel - 1);

    public void setDuration(float durationToSet) =>
      duration = Mathf.Clamp(durationToSet, 0.01f, float.MaxValue);

    public void setStartTime(float timeToSet, float lowerBound = 0) {
      if (element.element != null) {
         startTime = Mathf.Clamp(timeToSet, lowerBound, float.MaxValue);
      }
    }
    
    public void unlink() {
      linkedNode = F.none_;
      convert(Element.At.SpecificTime);
    }

    public void refreshColor() =>
      nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;

    public void setTimeOffset(float time) => element.timeOffset = time;

    public TimelineNode(Element element, float startTime) {
      if (element.element != null && false) { // TODO
        isCallback = true;
        duration = 0;
      }
      else {
        isCallback = false;
        duration = element.element?.toTimelineElement().duration ?? 10;
      }
      this.element = element;
      channel = element.timelineChannelIdx;
      this.startTime = startTime;
      nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;
    }

    static Color elementToColor(ISerializedTweenTimelineElement element) {
      return element switch {
        LocalScale _ => new Color(0.75f, 0.25f, 1),
        AnchoredPosition _ => Color.yellow,
        _ => Color.white
      };
      // switch (element) {
      //   case Transform_Position _:         return Color.yellow;
      //   case Path_Transfrom_Position _:    return Color.cyan;
      //   case Transform_LocalEulerAngles _: return new Color(1f, 0.75f, 1f);
      //   case Transform_Rotation _:         return Color.green;
      //   case Transform_LocalScale _:       return new Color(0.75f, 0.25f, 1);
      //   case Light_Color _:                return new Color(0.75f, 1, 1);
      //   case Light_Intensity _:            return new Color(1, 1, 0.75f);
      //   case Graphic_ColorAlpha _:         return new Color(0.25f, 0.75f, 1f);
      //   case Graphic_Color _:              return new Color(1f, 0.5f, 0f);
      //   default:                           return Color.white;
      // }
    }
    
    public void convert(Element.At newStartType) {
      if (element.element != null) {
        switch (newStartType) {
          case Element.At.AfterLastElement: {
            if (linkedNode.valueOut(out var linked)) {
              setTimeOffset(startTime - linked.getEnd());
              element.startAt = newStartType;
              setDuration(duration);
            }

            break;
          }
          case Element.At.SpecificTime: {
            setTimeOffset(startTime);
            element.startAt = newStartType;
            setDuration(duration);
            
            break;
          }
          case Element.At.WithLastElement: {
            if (linkedNode.valueOut(out var linked)) {
              setTimeOffset(startTime - linked.startTime);
              element.startAt = newStartType;
              setDuration(duration);
            }

            break;
          }
        }
      }
    }
    
  }
}
#endif
