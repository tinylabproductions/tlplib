#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.io {
  public class PrintWriter : Writer {
    public PrintWriter(AndroidJavaObject java) : base(java) {}

    // ReSharper disable once SuggestBaseTypeForParameter
    public PrintWriter(Writer writer) 
      : this(new AndroidJavaObject("java.io.PrintWriter", writer.java)) {}
  }
}
#endif