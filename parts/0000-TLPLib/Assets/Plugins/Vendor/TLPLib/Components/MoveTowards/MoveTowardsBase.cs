using System;
using AdvancedInspector;
using Assets.modules.code_events.Events;
using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Plugins.Vendor.TLPLib.Components.MoveTowards {
  public abstract class MoveTowardsBase : MonoBehaviour, IMB_OnEnable, IMB_OnDisable, IMB_Awake {

#pragma warning disable 649
    [SerializeField, NotNull] protected Transform target;
    [SerializeField, CreateDerived] CodeEvent[] onFinish;
#pragma warning restore 649

    protected Transform t;
    IDisposable movement = Subscription.empty;

    public void Awake() => t = transform;

    public void OnEnable() {
      var elapsedTime = 0f;

      movement = ASync.EveryFrame(this, () => {
        if (Mathf.Approximately((t.position - target.position).magnitude, 0)) {
          onFinish?.invoke();
          return false;
        }
        var deltaTime = Time.deltaTime;
        run(deltaTime, elapsedTime);
        elapsedTime += deltaTime;
        return true;
      });
    }

    public void OnDisable() => movement.Dispose();

    protected abstract void run(float deltaTime, float elapsedTime);
  }
}