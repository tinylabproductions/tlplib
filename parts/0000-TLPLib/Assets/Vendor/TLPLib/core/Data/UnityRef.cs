using System;

namespace com.tinylabproductions.TLPLib.Data {
  public static class UnityRef {
    public static UnityRef<A> a<A>(A a) where A : UnityEngine.Object =>
      new UnityRef<A>(a);
  }

  /// <summary>
  /// Reference to an unity object, mainly for it to implement <see cref="IDisposable"/>.
  /// </summary>
  /// <typeparam name="A"></typeparam>
  public class UnityRef<A> : IDisposable where A : UnityEngine.Object {
    public A reference { get; private set; }

    public UnityRef(A reference) {
      this.reference = reference;
    }

    public override string ToString() => $"{nameof(UnityRef<A>)}({reference})";

    // Not sure, should we destroy the object here?
    // -- arturaz
    public void Dispose() => reference = null;

    public static implicit operator A(UnityRef<A> r) => r.reference;
  }
}