#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class Boolean : Binding {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("java.lang.Boolean");

    public Boolean(AndroidJavaObject java) : base(java) {}

    public Boolean(bool val) : this(klass.csjo("valueOf", val)) {}
  }
}
#endif