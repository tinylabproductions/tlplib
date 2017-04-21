using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class IObservableTestSubscribeWhileAlive : MonoBehaviour {
      #region Unity Serialized Fields

#pragma warning disable 649
      [SerializeField, NotNull] GameObject go1, go2;
#pragma warning restore 649

      #endregion

    void Awake() => destroyGOAndLog(go1, $"{nameof(Awake)}: ");
    void Start() => destroyGOAndLog(go2, $"{nameof(Start)}: ");

    static void destroyGOAndLog(GameObject go, string s) {
      var sub = new Subject<Unit>().subscribeWhileAlive(go, _ => { });
      Debug.LogWarning(s + sub.isSubscribed);
      Destroy(go);
      ASync.NextFrame(() =>
            Debug.LogWarning(s + sub.isSubscribed)
      );
    }
  }
}
