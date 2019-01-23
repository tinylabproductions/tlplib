using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public enum UnityPhase : byte { Update, LateUpdate, FixedUpdate }
  
  /// <summary>
  /// <see cref="MonoBehaviour"/> that runs our <see cref="TweenManager"/>s.
  /// </summary>
  [AddComponentMenu("")]
  public class TweenManagerRunner : MonoBehaviour, IMB_Update, IMB_FixedUpdate, IMB_LateUpdate {
    static TweenManagerRunner _instance;
    [PublicAPI] public static TweenManagerRunner instance {
      get {
        TweenManagerRunner create() {
          var go = new GameObject(nameof(TweenManagerRunner));
          DontDestroyOnLoad(go);
          return go.AddComponent<TweenManagerRunner>();
        }

        return _instance ? _instance : (_instance = create());
      }
    }

    [PublicAPI] public UnityPhase phase { get; private set; }

    public int currentlyRunningTweenCount =>
      onUpdate.total
      + onUpdateUnscaled.total
      + onFixedUpdate.total
      + onLateUpdate.total
      + onLateUpdateUnscaled.total;

    class Tweens {
      readonly HashSet<TweenManager>
        current = new HashSet<TweenManager>(),
        toAdd = new HashSet<TweenManager>(),
        toRemove = new HashSet<TweenManager>();

      bool running;
      public int total => current.Count;

      public void add(TweenManager tm) {
        // If we made a call to add a tween on the same phase
        // as we are running the tween, we want to set it's state to zero
        // and run on the next frame.
        if (phaseEqualsTweenTime(instance.phase, tm.time)) {
          var timeline = tm.timeline;
          timeline.applyStateAt(timeline.timePassed);
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
          foreach (var t in current) {
            if (!t.update(deltaTime)) {
              if (t.stopIfDestroyed) {
                if (Log.d.isWarn()) Log.d.warn($"Tween stopped because target was destroyed. Context: {t.context}");
                toRemove.Add(t);
              }
              else Log.d.error($"Tween target was destroyed. Context: {t.context}");
            }
          }
        }
        finally {
          running = false;

          if (toRemove.Count > 0) {
            foreach (var tween in toRemove) {
              tween.Dispose();
              current.Remove(tween);
            }
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