#if UNITY_EDITOR
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  public class FunSequenceNode {
    public SerializedTweenTimeline.Element element;
    public SerializedTweenTimeline.Element.At startType;
    public float startTime, duration;
    public int channel;
    public string name;
    public bool isCallback;
    public Color nodeTextColor;

    public Option<FunSequenceNode> linkedNode { get; private set; }

    public void changeDuration() {
      if (!isCallback) { element.element.setDuration(duration);}
    }

    public void linkNodeTo(FunSequenceNode linkTo) { linkedNode = linkTo.some(); }

    public void unlink() {
      linkedNode = F.none_;
      setSelectedNodeElementFields(SerializedTweenTimeline.Element.At.SpecificTime);
    }
    
    public float getEnd() => startTime + duration;

    public void setSpecificTimeStart() {
      if (element.startAt == SerializedTweenTimeline.Element.At.SpecificTime) {
        element.timeOffset = startTime;
      }
    }

    public void reLink(FunSequenceNode linkTo) {
      if (element.startAt == SerializedTweenTimeline.Element.At.AfterLastElement) {
        linkNodeTo(linkTo);
      }
    }

    public void refreshColor() =>
      nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;

    public void setTimeOffset(float time) { element.timeOffset = time; }

    public void resetTimeOffset() => element.timeOffset = 0f;
 
    public FunSequenceNode(SerializedTweenTimeline.Element element,  float startTime, string name) {
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
      startType = element.startAt;
      this.startTime = startTime;
      this.name = name;
      nodeTextColor = element.element != null ? elementToColor(element.element) : Color.white;
    }

    static Color elementToColor(SerializedTweenTimelineElement element) {
      switch ( (object)element ) {
        case Transform_Position _:
          return Color.yellow;
        case Path_Transfrom_Position _:
          return Color.cyan;
        case Transform_LocalEulerAngles _:
          return new Color(1f, 0.75f, 1f);
        case Transform_Rotation _:
          return Color.green;
        case Transform_LocalScale _:
          return new Color(0.75f, 0.25f, 1);
        case Light_Color _:
          return new Color(0.75f, 1, 1);
        case Light_Intensity _:
          return new Color(1, 1, 0.75f);
        case Graphic_ColorAlpha _:
          return new Color(0.25f, 0.75f, 1f);
        case Graphic_Color _:
          return new Color(1f, 0.5f, 0f);
        default: return Color.white;
      }
    }
    
    public void setSelectedNodeElementFields(SerializedTweenTimeline.Element.At newStartType) {
      if (element.element != null) {
        switch (newStartType) {
          case SerializedTweenTimeline.Element.At.AfterLastElement: {
            if (linkedNode.valueOut(out var linked)) {
              setTimeOffset(startTime - linked.getEnd());
              element.startAt = newStartType;
              element.element.setDuration(duration);
            }

            break;
          }
          case SerializedTweenTimeline.Element.At.SpecificTime: {
            setTimeOffset(startTime);
            element.startAt = newStartType;
            element.element.setDuration(duration);
            break;
          }
          case SerializedTweenTimeline.Element.At.WithLastElement: {
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