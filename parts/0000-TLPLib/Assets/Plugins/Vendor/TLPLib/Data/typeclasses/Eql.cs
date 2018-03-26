using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public interface Eql<in A> {
    [PublicAPI]
    bool eql(A a1, A a2);
  }

  public static class Eql {
    [PublicAPI]
    public static IEqualityComparer<A> asEqualityComparer<A>(this Eql<A> eql) => new AsEqualityComparer<A>(eql);
    
    [PublicAPI]
    public class Lambda<A> : Eql<A> {
      readonly Fn<A, A, bool> _eql;

      public Lambda(Fn<A, A, bool> eql) {
        _eql = eql;
      }

      public bool eql(A a1, A a2) => _eql(a1, a2);
    }

    class AsEqualityComparer<A> : IEqualityComparer<A> {
      readonly Eql<A> eql;

      public AsEqualityComparer(Eql<A> eql) {
        this.eql = eql;
      }

      public bool Equals(A x, A y) => eql.eql(x, y);
      public int GetHashCode(A obj) => obj.GetHashCode();
    }
  }
}