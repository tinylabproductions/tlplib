using System;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public interface Eql<in A> {
    bool eql(A a1, A a2);
  }

  public static class Eql {
    public class Lambda<A> : Eql<A> {
      readonly Fn<A, A, bool> _eql;

      public Lambda(Fn<A, A, bool> eql) {
        _eql = eql;
      }

      public bool eql(A a1, A a2) => _eql(a1, a2);
    }
  }
}