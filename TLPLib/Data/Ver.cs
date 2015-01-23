using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Data {
  /* This data type is useful when we have mutable data in reactive references.
   * Because RxVal/Ref compares values before dispatching, we have no way to 
   * dispatch a change in the mutable structure.
   * 
   * For example:
   * <code>
   *   var list = new List<int>();
   *   var rx = RxRef.a(list);
   *   list.Add(1);
   *   rx.value = list; // Does nothing, because it's the same list.
   * </code>
   * 
   * Thus we need Ver to wrap it and indicate changes.
   * 
   * <code>
   *   var list = new List<int>();
   *   var rx = RxRef.a(Ver.a(list));
   *   list.Add(1);
   *   rx.value = rx.value.next;
   *   // or
   *   rx.value = Ver.a(list);
   * </code>
   * 
   */
  public struct Ver<A> : IEquatable<Ver<A>> {
    public readonly A value;
    public readonly uint version;

    #region Equality

    public bool Equals(Ver<A> other) {
      return EqualityComparer<A>.Default.Equals(value, other.value) && version == other.version;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Ver<A> && Equals((Ver<A>) obj);
    }

    public override int GetHashCode() {
      unchecked { return (EqualityComparer<A>.Default.GetHashCode(value) * 397) ^ (int) version; }
    }

    public static bool operator ==(Ver<A> left, Ver<A> right) { return left.Equals(right); }
    public static bool operator !=(Ver<A> left, Ver<A> right) { return !left.Equals(right); }

    sealed class ValueVersionEqualityComparer : IEqualityComparer<Ver<A>> {
      public bool Equals(Ver<A> x, Ver<A> y) {
        return EqualityComparer<A>.Default.Equals(x.value, y.value) && x.version == y.version;
      }

      public int GetHashCode(Ver<A> obj) {
        unchecked { return (EqualityComparer<A>.Default.GetHashCode(obj.value) * 397) ^ (int) obj.version; }
      }
    }

    static readonly IEqualityComparer<Ver<A>> ValueVersionComparerInstance = new ValueVersionEqualityComparer();

    public static IEqualityComparer<Ver<A>> valueVersionComparer {
      get { return ValueVersionComparerInstance; }
    }

    #endregion

    public Ver(A value) {
      this.value = value;
      version = Ver.nextCounter;
    }

    public Ver<A> next { get { return new Ver<A>(value); } } 
  }

  public static class Ver {
    static uint counter;
    public static uint nextCounter { get { return ++counter; } }

    public static Ver<A> a<A>(A value) { return new Ver<A>(value); }
  }
}
