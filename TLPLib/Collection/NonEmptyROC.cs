using System.Collections.Generic;
using System.Collections.ObjectModel;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class NonEmptyROC {
    public static NonEmptyROC<A> a<A>(A first, params A[] rest) 
    { return NonEmptyROC<A>.a(first, rest); }

    public static NonEmptyROC<A> a<A>(A first, IList<A> rest) 
    { return NonEmptyROC<A>.a(first, rest); }

    public static Option<NonEmptyROC<A>> fromList<A>(IList<A> list) 
    { return NonEmptyROC<A>.a(list); }
  }

  /* Non-empty Read-Only Collection */
  public class NonEmptyROC<A> : ReadOnlyCollection<A> {
    protected NonEmptyROC(IList<A> list) : base(list) { }

    public static NonEmptyROC<A> a(A first, IList<A> rest) {
      var arr = new A[rest.Count + 1];
      arr[0] = first;
      rest.CopyTo(arr, 1);
      return new NonEmptyROC<A>(arr);
    }

    public static Option<NonEmptyROC<A>> a(IList<A> list) 
    { return (!list.isEmpty()).opt(() => new NonEmptyROC<A>(list)); }
  }

  public static class NonEmptyROC2 {
    public static NonEmptyROC2<A> a<A>(A first, A second, params A[] rest) 
    { return NonEmptyROC2<A>.a(first, second, rest); }

    public static NonEmptyROC2<A> a<A>(A first, A second, IList<A> rest) 
    { return NonEmptyROC2<A>.a(first, second, rest); }

    public static NonEmptyROC2<A> a<A>(A first, NonEmptyROC<A> rest) 
    { return NonEmptyROC2<A>.a(first, rest); }

    public static Option<NonEmptyROC2<A>> fromList<A>(IList<A> list) 
    { return NonEmptyROC2<A>.a(list); }
  }

  /* Non-empty Read-Only Collection with at least 2 values*/
  public class NonEmptyROC2<A> : NonEmptyROC<A> {
    private NonEmptyROC2(IList<A> list) : base(list) { }

    public static NonEmptyROC2<A> a(A first, A second, IList<A> rest) {
      var arr = new A[rest.Count + 2];
      arr[0] = first;
      arr[1] = second;
      rest.CopyTo(arr, 2);
      return new NonEmptyROC2<A>(arr);
    }

    public static NonEmptyROC2<A> a(A first, NonEmptyROC<A> rest) {
      var arr = new A[rest.Count + 1];
      arr[0] = first;
      rest.CopyTo(arr, 1);
      return new NonEmptyROC2<A>(arr);
    }

    public new static Option<NonEmptyROC2<A>> a(IList<A> list) 
    { return (list.Count > 1).opt(() => new NonEmptyROC2<A>(list)); }
  }
}
