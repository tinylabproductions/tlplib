#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.os {
  public class BaseBundle : Binding {
    public BaseBundle(AndroidJavaObject java) : base(java) {}

    public void putBoolean(string key, bool value) => java.Call("putBoolean", key, value);
    public void putBooleanArray(string key, bool[] value) => java.Call("putBooleanArray", key, value);
    public void putDouble(string key, double value) => java.Call("putDouble", key, value);
    public void putDoubleArray(string key, double[] value) => java.Call("putDoubleArray", key, value);
    public void putInt(string key, int value) => java.Call("putInt", key, value);
    public void putIntArray(string key, int[] value) => java.Call("putIntArray", key, value);
    public void putLong(string key, long value) => java.Call("putLong", key, value);
    public void putLongArray(string key, long[] value) => java.Call("putLongArray", key, value);
    public void putString(string key, string value) => java.Call("putString", key, value);
    public void putStringArray(string key, string[] value) => java.Call("putStringArray", key, value);
    public void remove(string key) => java.Call("remove", key);
    public int size => java.Call<int>("size");
    public bool isEmpty => java.Call<bool>("isEmpty");
  }
}

#endif