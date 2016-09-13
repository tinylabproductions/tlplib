using System;
using System.Collections.Generic;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IListExts {
    public static A a<A>(this IList<A> list, int index) {
      if (index < 0 || index >= list.Count) throw new ArgumentOutOfRangeException(
        nameof(index), 
        $"Index invalid for IList<{typeof(A)}> (count={list.Count}): {index}"
      );
      return list[index];
    }

    public static Option<T> get<T>(this IList<T> list, int index) {
      return (index >= 0 && index < list.Count)
        ? F.some(list[index]) : F.none<T>();
    }

    public static List<A> reversed<A>(this List<A> list) {
      var reversed = new List<A>(list);
      reversed.Reverse();
      return reversed;
    }

    public static T updateOrAdd<T>(
      this IList<T> list, Fn<T, bool> finder, Fn<T> ifNotFound, Fn<T, T> ifFound
    ) {
      var idxOpt = list.indexWhere(finder);
      if (idxOpt.isEmpty) {
        var item = ifNotFound();
        list.Add(item);
        return item;
      }
      else {
        var idx = idxOpt.get;
        var updated = ifFound(list[idx]);
        list[idx] = updated;
        return updated;
      }
    }

    public static void updateWhere<T>(
      this IList<T> list, Fn<T, bool> finder, Fn<T, T> ifFound
    ) {
      var idxOpt = list.indexWhere(finder);
      if (idxOpt.isEmpty) return;

      var idx = idxOpt.get;
      list[idx] = ifFound(list[idx]);
    }

    public static void Shuffle<A>(this IList<A> list) {
      var rng = new System.Random();
      var n = list.Count;
      while (n > 1) {
        n--;
        var k = rng.Next(n + 1);
        var value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }

    public static bool isEmpty<A>(this IList<A> list) => list.Count == 0;

    public static bool nonEmpty<A>(this IList<A> list) => list.Count != 0;

    public static Option<A> random<A>(this IList<A> list)
      { return list.randomIndex().map(idx => list[idx]); }

    public static Option<int> randomIndex<A>(this IList<A> list) {
      return list.Count == 0
        ? F.none<int>() : F.some(Random.Range(0, list.Count));
    }

    public static void swap<A>(this IList<A> list, int aIndex, int bIndex) {
      var temp = list[aIndex];
      list[aIndex] = list[bIndex];
      list[bIndex] = temp;
    }

    public static IEnumerable<A> dropRight<A>(this IList<A> list, int count) {
      var end = list.Count - count;
      var idx = 0;
      foreach (var item in list) {
        if (idx < end) yield return item;
        idx++;
      }
    }

    public static IList<A> toIList<A>(this List<A> list) { return list; }

    /* Constructs a string from this List. Iterless version. */
    public static string mkString<A>(
      this IList<A> iter,
      string separator, string start = null, string end = null
    ) {
      var b = new StringBuilder();
      if (start != null) b.Append(start);
      for (var idx = 0; idx < iter.Count; idx++) {
        if (idx == 0) b.Append(iter[idx]);
        else {
          b.Append(separator);
          b.Append(iter[idx]);
        }
      }
      if (end != null) b.Append(end);

      return b.ToString();
    }

    public static Option<int> indexWhere<A>(this IList<A> list, Fn<A, bool> predicate) {
      for (var idx = 0; idx < list.Count; idx++)
        if (predicate(list[idx])) return F.some(idx);
      return F.none<int>();
    }

    /* Returns a random element. The probability is selected by elements
     * weight. Non-iter version. */
    public static Option<A> randomElementByWeight<A>(
      this IList<A> list,
      // If we change to Func here, Unity crashes. So fun.
      Fn<A, float> weightSelector
    ) {
      if (list.isEmpty()) return F.none<A>();

      var totalWeight = 0f;
      // ReSharper disable once LoopCanBeConvertedToQuery
      for (var idx = 0; idx < list.Count; idx++)
        totalWeight += weightSelector(list[idx]);

      // The weight we are after...
      var itemWeightIndex = Random.value * totalWeight;
      var currentWeightIndex = 0f;

      // ReSharper disable once ForCanBeConvertedToForeach
      for (var idx = 0; idx < list.Count; idx++) {
        var a = list[idx];
        currentWeightIndex += weightSelector(a);
        // If we've hit or passed the weight we are after for this item then it's the one we want....
        if (currentWeightIndex >= itemWeightIndex) return F.some(a);
      }

      throw new IllegalStateException();
    }

    public static Option<A> headOption<A>(this IList<A> list)
      { return list.Count == 0 ? F.none<A>() : list[0].some(); }
  }
}
