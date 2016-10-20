#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.io {
  public class StringWriter : Writer {
    public StringWriter(AndroidJavaObject java) : base(java) {}
    public StringWriter() : this(new AndroidJavaObject("java.io.StringWriter")) {}
  }
}
#endif