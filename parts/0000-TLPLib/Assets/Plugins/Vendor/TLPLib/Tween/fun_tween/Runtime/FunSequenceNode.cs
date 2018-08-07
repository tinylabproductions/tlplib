#if UNITY_EDITOR
using System;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;

namespace com.tinylabproductions.TLPLib.Editor.VisualTweenTimeline {
  [Serializable]
  public class FunSequenceNode {
    public SerializedTweenTimeline.Element element;
    public SerializedTweenTimeline.Element.At startType;
    public float startTime, duration;
    public int channel;
    public string name;
    
    bool update;

    public void changeDuration() => element.element.setDuration(duration);
    public float getEnd() => startTime + duration;

    public void setSpecificTimeStart() {
      if (element.startAt == SerializedTweenTimeline.Element.At.SpecificTime) {
        element.timeOffset = startTime;
      }
    }

  

    public void setTimeOffset(float time) { element.timeOffset = time; }

    public void resetTimeOffset() => element.timeOffset = 0f;
 
    public FunSequenceNode(SerializedTweenTimeline.Element element,  float startTime, string name) {
      this.element = element;
      channel = element.timelineChannelIdx;
      startType = element.startAt;
      //TODO not sure about this
      duration = element.element ? element.element.duration : 2;
      this.startTime = startTime;
      this.name = name;
    }
  }
}
#endif