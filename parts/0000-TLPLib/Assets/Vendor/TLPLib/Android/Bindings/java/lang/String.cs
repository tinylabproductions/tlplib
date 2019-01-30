#if UNITY_ANDROID
using System.Text;
using com.tinylabproductions.TLPLib.Android.Bindings.java.nio.charset;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class String : Binding {
    public String(AndroidJavaObject java) : base(java) {}

    public String(string s) : this(new AndroidJavaObject(
      "java.lang.String", Encoding.UTF8.GetBytes(s), Charset.UTF_8.java
    )) {}
  }
}
#endif