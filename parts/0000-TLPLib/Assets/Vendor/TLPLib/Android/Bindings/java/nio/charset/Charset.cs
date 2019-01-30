#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.nio.charset {
  public class Charset : Binding {
    public static readonly Charset UTF_8;

    static readonly AndroidJavaClass klass;

    static Charset() {
      klass = new AndroidJavaClass("java.nio.charset.Charset");
      // StandardCharsets are not available in old androids.
      UTF_8 = forName("UTF-8");
    }

    public Charset(AndroidJavaObject java) : base(java) {}

    public static Charset forName(string charsetName) =>
      new Charset(klass.csjo("forName", charsetName));
  }
}
#endif