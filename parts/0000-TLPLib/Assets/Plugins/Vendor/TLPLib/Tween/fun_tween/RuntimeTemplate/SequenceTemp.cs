using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
  [System.Serializable]
  public class SequenceTemp {
    public string name = "New SequenceTemp";
    public SequenceWrap wrap = SequenceWrap.ClampForever;
    public bool playAutomatically = true;
    public List<SequenceNodeTemp> nodes;
    public List<EventNodeTemp> events;

    public float passedTime;
    private bool playForward = true;
    public bool stop = true;

    private float sequenceEnd;
    private float time;

    public void Update(GameObject go) {
      if (stop) {
        return;
      }

      if (Time.time > time) {
        switch (wrap) {
          case SequenceWrap.PingPong:
            time = Time.time + sequenceEnd;
            playForward = !playForward;
            ResetEvents();
            break;
          case SequenceWrap.Once:
            Stop(false);
            break;
          case SequenceWrap.ClampForever:
            Stop(true);
            break;
          case SequenceWrap.Loop:
            Restart();
            break;
        }
      }
      else {
        passedTime += Time.deltaTime * (playForward ? 1.0f : -1.0f);

        foreach (SequenceNodeTemp node in nodes) {
          node.UpdateTween(passedTime);
        }
      }

      foreach (EventNodeTemp node in events) {
        if (passedTime >= node.time) {
          node.Invoke(go);
        }
      }
    }

    public void Play() {
      stop = false;
      passedTime = 0;
      foreach (SequenceNodeTemp node in nodes) {
        if (sequenceEnd < (node.startTime + node.duration)) {
          sequenceEnd = node.startTime + node.duration;
        }
      }

      ResetEvents();
      time = Time.time + sequenceEnd;
    }

    public void Stop() { stop = true; }

    public void Stop(bool forward) {
      stop = true;
      for (int i = 0; i < nodes.Count; i++) {
        SequenceNodeTemp nodeTemp = nodes[i];
        if (forward) {
          nodeTemp.UpdateValue(1.0f);
        }
        else {
          nodeTemp.UpdateValue(0.0f);
          passedTime = 0;
        }
      }
    }

    public void Restart() {
      Stop(false);
      Play();
    }

    private void ResetEvents() {
      foreach (EventNodeTemp node in events) {
        node.finished = false;
      }
    }
  }

  public enum SequenceWrap {
    Once,
    PingPong,
    Loop,
    ClampForever
  }
}