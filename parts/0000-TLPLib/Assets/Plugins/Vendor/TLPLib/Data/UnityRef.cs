using System;

namespace com.tinylabproductions.TLPLib.Data {
  public static class UnityRef {
    public static UnityRef<A> a<A>(A a) where A : UnityEngine.Object =>
      new UnityRef<A>(a); 
  }

  public class UnityRef<A> : IDisposable where A : UnityEngine.Object {
    public A reference { get; private set; }

    public UnityRef(A reference) {
      this.reference = reference;
    }

    public override string ToString() => $"{nameof(UnityRef<A>)}({reference})";

    public void Dispose() => reference = null;

    public static implicit operator A(UnityRef<A> r) => r.reference;
  }
}