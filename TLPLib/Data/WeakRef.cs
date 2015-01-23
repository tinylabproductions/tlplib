using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  struct WeakRef<A> where A : class {
    readonly WeakReference reference;

    public WeakRef(A target) { reference = new WeakReference(target); }

    public A target {
      get { return (A) reference.Target; }
      set { reference.Target = value; }
    }

    public Option<A> targetOpt {
      get { return F.opt((A) reference.Target); }
    }

    public bool isAlive { get { return reference.IsAlive; } }
  }
}
