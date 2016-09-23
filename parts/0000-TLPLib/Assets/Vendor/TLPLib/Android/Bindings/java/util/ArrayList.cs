#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.util {
  public class ArrayList : Binding {
    public ArrayList(AndroidJavaObject java) : base(java) {}
    public ArrayList(int capacity) 
      : this(new AndroidJavaObject("java.util.ArrayList", capacity)) {}
    public ArrayList() : this(0) {}

    public void add(int location, Binding o) => add(location, o.java);
    public void add(int location, AndroidJavaObject o) => java.Call("add", location, o);

    public bool add(Binding o) => add(o.java);
    public bool add(AndroidJavaObject o) => java.Call<bool>("add", o);
  }
}
#endif