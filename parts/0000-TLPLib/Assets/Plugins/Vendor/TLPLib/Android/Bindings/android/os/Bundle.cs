#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.os {
  public class Bundle : BaseBundle {
    public Bundle() : base(new AndroidJavaObject("android.os.Bundle")) {}
    public Bundle(AndroidJavaObject java) : base(java) {}

    public void putByte(string key, byte value) => java.Call("putByte", key, value);
    public void putByteArray(string key, byte[] value) => java.Call("putByteArray", key, value);
    public void putChar(string key, char value) => java.Call("putChar", key, value);
    public void putCharArray(string key, char[] value) => java.Call("putCharArray", key, value);
    public void putFloat(string key, float value) => java.Call("putFloat", key, value);
    public void putFloatArray(string key, float[] value) => java.Call("putFloatArray", key, value);
    public void putShort(string key, short value) => java.Call("putShort", key, value);
    public void putShortArray(string key, short[] value) => java.Call("putShortArray", key, value);
  }
}
#endif