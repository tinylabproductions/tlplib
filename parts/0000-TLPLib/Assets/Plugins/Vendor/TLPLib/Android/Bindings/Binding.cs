#if UNITY_ANDROID
using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings {
  public abstract class Binding : IEquatable<Binding>, IDisposable {
    public readonly AndroidJavaObject java;

    protected Binding(AndroidJavaObject java) { this.java = java; }

    public override string ToString() => java.Call<string>("toString");
    public override int GetHashCode() => java.Call<int>("hashCode");
    public override bool Equals(object obj) => Equals(obj as Binding);
    public void Dispose() => java.Dispose();

    public bool Equals(Binding other) {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      if (ReferenceEquals(java, other.java)) return true;
      return java.Call<bool>("equals", other.java);
    }

    public static bool operator ==(Binding left, Binding right) => Equals(left, right);
    public static bool operator !=(Binding left, Binding right) => !Equals(left, right);

    public static implicit operator AndroidJavaObject(Binding b) => b.java;
  }
}
#endif