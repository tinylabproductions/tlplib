using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tween_callbacks {
  [AddComponentMenu("")]
  public class Callback_UnityEvent : SerializedTweenCallback {
#pragma warning disable 649
    [SerializeField] InvokeOn _invokeOn;
    [SerializeField, NotNull] UnityEvent _onEvent;
#pragma warning restore 649
    protected override TweenCallback createCallback() => 
      new TweenCallback(evt => {
        if (shouldInvoke(_invokeOn, evt)) _onEvent.Invoke();
      });
    
    public override string ToString() => $"Unity Event @ {_invokeOn}";
  }
}