#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class Integer : Binding {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("java.lang.Integer");

    public Integer(AndroidJavaObject java) : base(java) {}

    public Integer(int val) : this(klass.csjo("valueOf", val)) {}
  }
}
#endif