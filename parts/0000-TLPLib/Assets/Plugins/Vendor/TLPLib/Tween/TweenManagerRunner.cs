﻿using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween {
  public enum UnityPhase : byte { Update, LateUpdate, FixedUpdate }
  /// <summary>
  /// <see cref="MonoBehaviour"/> that runs our <see cref="TweenManager"/>s.
  /// </summary>
  public class TweenManagerRunner : MonoBehaviour, IMB_Update, IMB_FixedUpdate, IMB_LateUpdate {
    public static readonly TweenManagerRunner instance;
    public UnityPhase phase { get; private set; }

    static TweenManagerRunner() {
      if (instance == null) {
        var go = new GameObject(nameof(TweenManagerRunner));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<TweenManagerRunner>();
      }
    }

    class Tweens {
      readonly HashSet<TweenManager> 
        current = new HashSet<TweenManager>(), 
        toAdd = new HashSet<TweenManager>(), 
        toRemove = new HashSet<TweenManager>();

      bool running;

      public void add(TweenManager tm) {
        // If we made a call to add a tween on the same phase
        // as we are running the tween, we want to set it's state to zero
        // and run on the next frame.
        if (phaseEqualsTweenTime(instance.phase, tm.time)) {
          tm.update(0);
        }

        if (running) {
          // If we just stopped, but immediatelly restarted, just delete the pending removal.
          if (!toRemove.Remove(tm))
            // Otherwise schedule for addition.
            toAdd.Add(tm);
        }
        else {
          current.Add(tm);
        }
      }

      public void remove(TweenManager tm) {
        if (running) {
          if (!toAdd.Remove(tm))
            toRemove.Add(tm);
        }
        else {
          current.Remove(tm);
        }
      }

      public void runOn(float deltaTime) {
        try {
          running = true;
          foreach (var t in current)
            t.update(deltaTime);
        }
        finally {
          running = false;

          if (toRemove.Count > 0) {
            foreach (var tween in toRemove)
              current.Remove(tween);
            toRemove.Clear();
          }

          if (toAdd.Count > 0) {
            foreach (var tweenToAdd in toAdd)
              current.Add(tweenToAdd);
            toAdd.Clear();
          }
        }
      }
    }

    readonly Tweens 
      onUpdate = new Tweens(),
      onUpdateUnscaled = new Tweens(),
      onFixedUpdate = new Tweens(),
      onLateUpdate = new Tweens(),
      onLateUpdateUnscaled = new Tweens();

    TweenManagerRunner() { }

    public void Update() {
      phase = UnityPhase.Update;
      onUpdate.runOn(Time.deltaTime);
      onUpdateUnscaled.runOn(Time.unscaledDeltaTime);
    }

    public void LateUpdate() {
      phase = UnityPhase.LateUpdate;
      onLateUpdate.runOn(Time.deltaTime);
      onLateUpdateUnscaled.runOn(Time.unscaledDeltaTime);
    }

    public void FixedUpdate() {
      phase = UnityPhase.FixedUpdate;
      onFixedUpdate.runOn(Time.fixedDeltaTime);
    }

    public void add(TweenManager tweenManager) => 
      lookupSet(tweenManager.time).add(tweenManager);

    public void remove(TweenManager tweenManager) => 
      lookupSet(tweenManager.time).remove(tweenManager);

    Tweens lookupSet(TweenTime time) {
      switch (time) {
        case TweenTime.OnUpdate:             return onUpdate;
        case TweenTime.OnUpdateUnscaled:     return onUpdateUnscaled;
        case TweenTime.OnLateUpdate:         return onLateUpdate;
        case TweenTime.OnLateUpdateUnscaled: return onLateUpdateUnscaled;
        case TweenTime.OnFixedUpdate:        return onFixedUpdate;
        default: throw new ArgumentOutOfRangeException(nameof(time), time, null);
      }
    }

    static bool phaseEqualsTweenTime(UnityPhase phase, TweenTime time) {
      switch (time) {
        case TweenTime.OnUpdate:
        case TweenTime.OnUpdateUnscaled:
          if (phase == UnityPhase.Update) return true;
          break;
        case TweenTime.OnLateUpdate:
        case TweenTime.OnLateUpdateUnscaled:
          if (phase == UnityPhase.LateUpdate) return true;
          break;
        case TweenTime.OnFixedUpdate:
          if (phase == UnityPhase.FixedUpdate) return true;
          break;
        default: throw new ArgumentOutOfRangeException();
      }
      return false;
    }
  }
}