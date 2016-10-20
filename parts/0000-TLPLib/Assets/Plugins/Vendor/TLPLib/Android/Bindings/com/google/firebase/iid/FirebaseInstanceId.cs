#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.firebase.iid {
  public class FirebaseInstanceId : Binding {
    static readonly AndroidJavaClass klass = 
      new AndroidJavaClass("com.google.firebase.iid.FirebaseInstanceId");

    FirebaseInstanceId(AndroidJavaObject java) : base(java) {}

    public static readonly FirebaseInstanceId instance = 
      new FirebaseInstanceId(klass.csjo("getInstance"));

    public string token => java.Call<string>("getToken");
  }
}
#endif