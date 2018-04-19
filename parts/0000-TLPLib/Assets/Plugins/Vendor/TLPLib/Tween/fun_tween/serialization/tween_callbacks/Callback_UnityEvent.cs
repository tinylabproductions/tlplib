using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  [AddComponentMenu("")]
  public class Callback_UnityEvent : SerializedTweenCallback {
    [SerializeField, NotNull] UnityEvent _event;
    
    public override TweenCallback callback { get; }

    Callback_UnityEvent() {
      callback = new TweenCallback(evt => _event.Invoke());
    }
  }
}