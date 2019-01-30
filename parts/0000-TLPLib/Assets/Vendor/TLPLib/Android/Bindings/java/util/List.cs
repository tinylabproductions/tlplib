#if UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.util {
  public class List : Binding, IEnumerable<AndroidJavaObject> {
    public List(AndroidJavaObject java) : base(java) { }

    public void add(int location, AndroidJavaObject o) => java.Call("add", location, o);
    public bool add(AndroidJavaObject o) => java.Call<bool>("add", o);

    public Iterator iterator() => new Iterator(java.cjo("iterator"));

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<AndroidJavaObject> GetEnumerator() {
      var iterator = this.iterator();
      while (iterator.hasNext) yield return iterator.next();
    }
  }
}
#endif