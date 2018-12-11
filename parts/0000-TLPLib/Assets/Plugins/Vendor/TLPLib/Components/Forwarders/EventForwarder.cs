using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class EventForwarder<A> : MonoBehaviour {
    protected readonly Subject<A> _onEvent = new Subject<A>();
    public IRxObservable<A> onEvent => _onEvent;
  }
}