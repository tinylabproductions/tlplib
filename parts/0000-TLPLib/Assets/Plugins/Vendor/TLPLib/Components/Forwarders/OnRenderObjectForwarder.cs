using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnRenderObjectForwarder : MonoBehaviour, IMB_OnRenderObject {
    readonly Subject<Camera> subject = new Subject<Camera>();
    public IObservable<Camera> onRenderObject => subject;

    public void OnRenderObject() {
      if (Log.isVerbose) Log.verbose(
        $"{nameof(OnRenderObjectForwarder)} this = {this}, camera current = {Camera.current}"
      );
      subject.push(Camera.current);
    }
  }
}