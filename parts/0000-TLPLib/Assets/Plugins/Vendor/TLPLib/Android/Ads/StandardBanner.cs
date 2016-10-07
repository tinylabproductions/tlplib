using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
#if UNITY_ANDROID
  public class StandardBanner {
    protected readonly AndroidJavaObject java;

    public StandardBanner(AndroidJavaObject java) { this.java = java; }

    public void setVisibility(bool visible) {
      java.Call("setVisibility", visible);
    }

    public void load() {
      java.Call("load");
    }

    public void destroy() {
      java.Call("destroy");
    }
  }
#endif
}
