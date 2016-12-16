using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
#if UNITY_ANDROID
  public class StandardInterstitial {
    Subject<Unit> _onLoad = new Subject<Unit>();

    protected readonly AndroidJavaObject java;

    public IObservable<Unit> onLoad => _onLoad;

    public StandardInterstitial(AndroidJavaObject java) { this.java = java; }

    public void load() {
      java.Call("load");
      _onLoad.push(F.unit);
    }
    public bool ready => java.Call<bool>("isReady");
    public void show() { java.Call("show"); }
  }
#endif
}
