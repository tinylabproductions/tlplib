using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
#if UNITY_ANDROID
  public class StandardInterstitial {
    protected readonly AndroidJavaObject java;

    public StandardInterstitial(AndroidJavaObject java) { this.java = java; }

    public void load() { java.Call("load"); }
    public bool ready => java.Call<bool>("isReady");
    public void show() { java.Call("show"); }
  }
#endif
}
