#if UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.util {
  public class ArrayList : Binding {
    public ArrayList(AndroidJavaObject java) : base(java) {}
    public ArrayList(int capacity) 
      : this(new AndroidJavaObject("java.util.ArrayList", capacity)) {}
    public ArrayList() : this(0) {}

    public ArrayList(IEnumerable<AndroidJavaObject> enumerable, int capacity = 0) : this(capacity) {
      foreach (var elem in enumerable) add(elem);
    }

    public void add(int location, AndroidJavaObject o) => java.Call("add", location, o);
    public bool add(AndroidJavaObject o) => java.Call<bool>("add", o);
  }
}
#endif