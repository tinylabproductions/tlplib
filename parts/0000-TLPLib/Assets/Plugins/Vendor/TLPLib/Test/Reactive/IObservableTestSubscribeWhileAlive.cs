using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.Assertions;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class IObservableTestSubscribeWhileAlive : MonoBehaviour, IMB_Awake, IMB_Start {
    public void Awake() => WhenGameObjectDestroyed(new GameObject());
    public void Start() => WhenGameObjectDestroyed(new GameObject());

    static void WhenGameObjectDestroyed(GameObject go) {
      var sub = new Subject<Unit>().subscribeWhileAlive(go, _ => { });
      Assert.AreEqual(sub.isSubscribed, true);
      Destroy(go);
      ASync.NextFrame(() =>
        Assert.AreEqual(sub.isSubscribed, false)
      );
    }
  }
}
