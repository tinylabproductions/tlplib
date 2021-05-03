#if UNITY_EDITOR
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.manager;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners;
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
    }

    void setChannel(int idx) => channel = Mathf.Clamp(idx, 0, int.MaxValue);
    public void increaseChannel() => setChannel(channel + 1);
    public void decreaseChannel() => setChannel(channel - 1);

    public void setDuration(float durationToSet) =>
      duration = Mathf.Clamp(durationToSet, 0.01f, float.MaxValue);

    public void setStartTime(float timeToSet, float lowerBound = 0) {
      startTime = Mathf.Clamp(timeToSet, lowerBound, float.MaxValue);
    }
    
    public void unlink() {
      linkedNode = None._;
    }

    public void refreshColor() =>
      nodeTextColor = element.element?.editorColor ?? Color.white;

    public void setTimeOffset(float time) => element.setStartsAt(time);

    public TimelineNode(Element element) {
      // display invalid elements of length 1
      duration = element.element?.duration ?? 1;
      if (element.element is ISerializedTweenTimelineCallback) {
        isCallback = true;
      }
      else {
        isCallback = false;
      }
      this.element = element;
      channel = element.timelineChannelIdx;
      startTime = element.startsAt;
      nodeTextColor = element.element?.editorColor ?? Color.white;
    }
  }
}
#endif
