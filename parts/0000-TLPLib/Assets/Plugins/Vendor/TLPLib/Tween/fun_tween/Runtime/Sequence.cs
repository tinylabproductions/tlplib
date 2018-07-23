using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
  [System.Serializable]
  public class Sequence {
    public string name = "New Sequence";
    public SequenceWrap wrap = SequenceWrap.ClampForever;
    public bool playAutomatically = true;
    public List<SequenceNode> nodes;
    public List<EventNode> events;

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

        foreach (SequenceNode node in nodes) {
          node.UpdateTween(passedTime);
        }
      }

      foreach (EventNode node in events) {
        if (passedTime >= node.time) {
          node.Invoke(go);
        }
      }
    }

    public void Play() {
      stop = false;
      passedTime = 0;
      foreach (SequenceNode node in nodes) {
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
        SequenceNode node = nodes[i];
        if (forward) {
          node.UpdateValue(1.0f);
        }
        else {
          node.UpdateValue(0.0f);
          passedTime = 0;
        }
      }
    }

    public void Restart() {
      Stop(false);
      Play();
    }

    private void ResetEvents() {
      foreach (EventNode node in events) {
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