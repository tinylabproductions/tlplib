using com.tinylabproductions.TLPLib.Annotations;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class ScrollWheelObservable : MonoBehaviour {
    private readonly Subject<Vector2> _scrollDelta = new Subject<Vector2>();
    public IObservable<Vector2> scrollDelta { get { return _scrollDelta; } }

    [UsedImplicitly]
    private void Update() {
      var cur = Input.mouseScrollDelta;
      if (cur.x != 0 || cur.y != 0) _scrollDelta.push(cur);
    }
  }
}
