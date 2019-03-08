#if UNITY_ANDROID
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.os {
  [PublicAPI] public class Vibrator : Binding {
    public Vibrator(AndroidJavaObject java) : base(java) {}

    public void cancel() => java.Call("cancel");
    public bool hasVibrator => java.Call<bool>("hasVibrator");
    public void vibrate(long milliseconds) => java.Call("vibrate", milliseconds);
    public void vibrate(long[] pattern, int repeat) => java.Call("vibrate", pattern, repeat);
  }
}
#endif