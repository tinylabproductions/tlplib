using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
#if UNITY_ANDROID
  public interface IStandardInterstitial {
    void load();
    bool ready { get; }
    void show();
  }

  public class StandardInterstitial : IStandardInterstitial {
    readonly Subject<Unit> _onLoad = new Subject<Unit>();
    public IObservable<Unit> onLoad => _onLoad;

    protected readonly AndroidJavaObject java;

    public StandardInterstitial(AndroidJavaObject java) { this.java = java; }

    public void load() {
      java.Call("load");
      _onLoad.push(F.unit);
    }
    public bool ready => java.Call<bool>("isReady");
    public void show() => java.Call("show");
  }
#endif
}
