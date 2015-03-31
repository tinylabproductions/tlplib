using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IEnumerableExts {
    /* This should really be used only for debugging. It is pretty slow. */
    public static String asString(
      this IEnumerable enumerable, 
      bool newlines=true, bool fullClasses=false
    ) {
      var items = (
        from object item in enumerable
        let str = item as String // String is IEnumerable as well
        let enumItem = item as IEnumerable
        select str ?? (
          enumItem == null 
            ? item.debugObj() : enumItem.asString(newlines, fullClasses)
        )
      ).ToArray();
      var itemsStr = 
        string.Join(string.Format(",{0} ", newlines ? "\n " : ""), items);
      if (items.Length != 0 && newlines) itemsStr = "\n  " + itemsStr + "\n";

      var type = enumerable.GetType();
      return string.Format(
        "{0}[{1}]",
        fullClasses ? type.FullName : type.Name,
        itemsStr
      );
    }

    public static IEnumerable<A> Yield<A>(this A any) {
      yield return any;
    }

    public static Option<A> find<A>(this IEnumerable<A> enumerable, Fn<A, bool> predicate) {
      foreach (var a in enumerable) if (predicate(a)) return a.some();
      return F.none<A>();
    }

    public static IEnumerable<Tpl<A, int>> zipWithIndex<A>(this IEnumerable<A> enumerable) {
      var idx = 0;
      foreach (var a in enumerable) {
        yield return F.t(a, idx);
        idx += 1;
      }
    }

    public static Option<B> collectFirst<A, B>(
      this IEnumerable<A> enumerable, Fn<A, Option<B>> collector
    ) {
      foreach (var a in enumerable) {
        var opt = collector(a);
        if (opt.isDefined) return opt;
      }
      return F.none<B>();
    }

    public static Option<A> reduceLeft<A>(
      this IEnumerable<A> enumerable, Fn<A, A, A> f
    ) {
      if (enumerable.Any()) {
        return enumerable.Aggregate((a, b) => f(a, b)).some();
      }
      return F.none<A>();
    }
  }
}
