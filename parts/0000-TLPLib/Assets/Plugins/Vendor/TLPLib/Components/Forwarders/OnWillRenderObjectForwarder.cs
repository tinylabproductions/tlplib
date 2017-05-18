using com.tinylabproductions.TLPLib.Components.Interfaces;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components.Forwarders {
  public class OnWillRenderObjectForwarder : MonoBehaviour, IMB_OnWillRenderObject {
    readonly Subject<Camera> subject = new Subject<Camera>();
    public IObservable<Camera> onWillRenderObject => subject;

    public void OnWillRenderObject() {
      if (Log.isVerbose) Log.verbose(
        $"{nameof(OnWillRenderObjectForwarder)} this = {this}, camera current = {Camera.current}"
      );
      subject.push(Camera.current);
    }
  }
}