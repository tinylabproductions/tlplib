#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  [Serializable]
  public class FunSequenceNode {
    public SerializedTweenTimeline.Element element;
    public SerializedTweenTimeline.Element.At startType;
    public float startTime, duration;
    public int channel;
    public string name;
    public bool isCallback;

    Option<FunSequenceNode> _iIsLinkedToNode;

    public Option<FunSequenceNode> iIsLinkedToNode {
      get => _iIsLinkedToNode;
      private set => _iIsLinkedToNode = value;
    }

    public void changeDuration() {
      if (!isCallback) { element.element.setDuration(duration);}
    }

    public void linkNodeTo(FunSequenceNode linkTo) { iIsLinkedToNode = linkTo.some(); }
    public void unlink() { iIsLinkedToNode = F.none_; }
    
    public float getEnd() => startTime + duration;

    public void setSpecificTimeStart() {
      if (element.startAt == SerializedTweenTimeline.Element.At.SpecificTime) {
        element.timeOffset = startTime;
      }
    }

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
    }
    
    public void setSelectedNodeElementFields(SerializedTweenTimeline.Element.At newStartType) {
      if (element.element != null) {
        switch (newStartType) {
          case SerializedTweenTimeline.Element.At.AfterLastElement: {
            if (iIsLinkedToNode.valueOut(out var linkedNode)) {
              setTimeOffset(startTime - linkedNode.getEnd());
              element.startAt = newStartType;
              element.element.setDuration(duration);
            }

            break;
          }
          case SerializedTweenTimeline.Element.At.SpecificTime: {
            unlink();
            setTimeOffset(startTime);
            element.startAt = newStartType;
            element.element.setDuration(duration);
            break;
          }
          case SerializedTweenTimeline.Element.At.WithLastElement: {
            if (iIsLinkedToNode.valueOut(out var linkedNode)) {
              setTimeOffset(startTime - linkedNode.startTime);
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