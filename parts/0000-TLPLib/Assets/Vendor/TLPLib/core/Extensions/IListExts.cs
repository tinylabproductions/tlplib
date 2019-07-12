using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.exts;
using Smooth.Collections;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class IListExts {
    [PublicAPI]
    public static A a<A>(this IList<A> list, int index) {
      if (index < 0 || index >= list.Count) throw new ArgumentOutOfRangeException(
        nameof(index),
        $"Index invalid for IList<{typeof(A)}> (count={list.Count}): {index}"
      );
      return list[index];
    }

    [PublicAPI]
    public static bool getOut<A>(this IList<A> list, int index, out A a) {
      if (list.indexValid(index)) {
        a = list[index];
        return true;
      }
      else {
        a = default;
        return false;
      }
    }
    
    public static Option<T> get<T>(this IList<T> list, int index) =>
      list.indexValid(index) ? F.some(list[index]) : F.none<T>();

    [PublicAPI]
    public static T getOrElse<T>(this IList<T> list, int index, T defaultValue) =>
      list.indexValid(index) ? list[index] : defaultValue;

    [PublicAPI]
    public static Option<T> last<T>(this IList<T> list) => list.get(list.Count - 1);

    [PublicAPI]
    public static List<A> reversed<A>(this List<A> list) {
      var reversed = new List<A>(list);
      reversed.Reverse();
      return reversed;
    }

    [PublicAPI]
    public static T updateOrAdd<T>(
      this IList<T> list, Func<T, bool> finder, Func<T> ifNotFound, Func<T, T> ifFound
    ) {
      var idxOpt = list.indexWhere(finder);
      if (idxOpt.isNone) {
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
      this IList<T> list, Func<T, bool> finder, Func<T, T> ifFound
    ) {
      var idxOpt = list.indexWhere(finder);
      if (idxOpt.isNone) return;

      var idx = idxOpt.get;
      list[idx] = ifFound(list[idx]);
    }

    [PublicAPI] public static void shuffle<A>(this IList<A> list, ref Rng rng) {
      var n = list.Count;
      var range = new Range(0, n - 1);
      while (n > 1) {
        n--;
        var k = rng.nextIntInRange(range, out rng);
        var value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }

    public static bool isEmpty<A>(this IList<A> list) => list.Count == 0;
    public static bool nonEmpty<A>(this IList<A> list) => list.Count != 0;

    public static Option<A> random<A>(this IList<A> list) =>
      list.randomIndex().map(idx => list[idx]);

    public static Option<A> random<A>(this IList<A> list, ref Rng rng) =>
      list.randomIndex(ref rng).map(idx => list[idx]);

    public static Option<Tpl<Rng, A>> randomT<A>(this IList<A> list, Rng rng) =>
      list.randomIndexT(rng).map(t => t.map2(idx => list[idx]));

    public static A random<C, A>(this NonEmpty<C> list, ref Rng rng)
      where C : IReadOnlyList<A>
    =>
      list.a[rng.nextIntInRange(new Range(0, list.a.Count - 1), out rng)];

    public static A random<A>(this NonEmpty<ImmutableList<A>> list, ref Rng rng) =>
      random<ImmutableList<A>, A>(list, ref rng);

    public static A random<A>(this NonEmpty<ImmutableArray<A>> list, ref Rng rng) =>
      random<ImmutableArray<A>, A>(list, ref rng);

    public static Option<int> randomIndex<A>(this IList<A> list) =>
      list.Count == 0 ? F.none<int>() : F.some(Random.Range(0, list.Count));

    public static Option<int> randomIndex<A>(this IList<A> list, ref Rng rng) =>
      list.Count == 0
      ? F.none<int>()
      : F.some(rng.nextIntInRange(new Range(0, list.Count - 1), out rng));

    public static Option<Tpl<Rng, int>> randomIndexT<A>(this IList<A> list, Rng rng) =>
      list.Count == 0
      ? F.none<Tpl<Rng, int>>()
      : F.some(rng.nextIntInRangeT(new Range(0, list.Count - 1)));

    public static void swap<A>(this IList<A> list, int aIndex, int bIndex) {
      var temp = list[aIndex];
      list[aIndex] = list[bIndex];
      list[bIndex] = temp;
    }

    public static IEnumerable<A> dropRight<A>(this IList<A> list, int count) {
      var end = list.Count - count;
      var idx = 0;
      foreach (var item in list) {
        if (idx < end) {
          yield return item;
          idx++;
        }
        else {
          break;
        }
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

    /// <summary>Any that is garbage free.</summary>
    public static bool anyGCFree<A>(this IList<A> coll, Func<A, bool> predicate) {
      // ReSharper disable once ForCanBeConvertedToForeach, LoopCanBeConvertedToQuery
      for (var idx = 0; idx < coll.Count; idx++)
        if (predicate(coll[idx])) return true;
      return false;
    }

    public static Option<int> indexWhere<A>(this IList<A> list, Func<A, bool> predicate) {
      for (var idx = 0; idx < list.Count; idx++)
        if (predicate(list[idx])) return F.some(idx);
      return F.none<int>();
    }

    public static Option<int> indexWhereReverse<A>(this IList<A> list, Func<A, bool> predicate) {
      for (var idx = list.Count - 1; idx >= 0; idx--)
        if (predicate(list[idx])) return F.some(idx);
      return F.none<int>();
    }

    /* Returns a random element. The probability is selected by elements
     * weight. Non-iter version. */
    public static Option<A> randomElementByWeight<A>(
      this IList<A> list,
      // If we change to Func here, Unity crashes. So fun.
      Func<A, float> weightSelector
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

    public static Option<A> headOption<A>(this IList<A> list) => 
      list.Count == 0 ? F.none<A>() : list[0].some();

    /// <summary>
    /// Returns array with all the indexes of this list.
    /// </summary>
    public static int[] indexes<A>(this IList<A> list) {
      var indexes = new int[list.Count];
      for (var idx = 0; idx < list.Count; idx++)
        indexes[idx] = idx;
      return indexes;
    }

    public static IEnumerable<A> randomized<A>(this IList<A> list, Rng rng) {
      var indexes = list.indexes();
      indexes.shuffle(ref rng);
      foreach (var index in indexes)
        yield return list[index];
    }

    public static Option<int> indexOf<A>(
      this IList<A> list, A a, int startAt = 0, int? count = null, IEqualityComparer<A> comparer = null
    ) =>
      list.indexOfOut(a, out var idx, startAt: startAt, count: count, comparer: comparer)
        ? F.some(idx)
        : Option<int>.None;

    public static bool indexOfOut<A>(
      this IList<A> list, A a, out int index,
      int startAt = 0, int? count = null, IEqualityComparer<A> comparer = null
    ) => indexOfOutC(list: list, a: a, index: out index, startAt: startAt, count: count, comparer: comparer);

    public static bool indexOfOutC<A, C>(
      this C list, A a, out int index, 
      int startAt = 0, int? count = null, IEqualityComparer<A> comparer = null
    ) where C : IList<A> {
      comparer = comparer ?? EqComparer<A>.Default;
      var endIdx = startAt + count.GetValueOrDefault(list.Count - startAt);
      for (index = startAt; index < endIdx; index++) {
        if (comparer.Equals(list[index], a)) return true;
      }
      return false;
    }

    public static bool indexWhereOut<A>(
      this IList<A> list, Func<A, bool> predicate, out int index,
      int startAt = 0, int? count = null
    ) => indexWhereOutC(list, predicate, out index, startAt: startAt, count: count);

    public static bool indexWhereOutC<A, Coll>(
      this Coll list, Func<A, bool> predicate, out int index, 
      int startAt = 0, int? count = null
    ) where Coll : IList<A> {
      var endIdx = startAt + count.GetValueOrDefault(list.Count - startAt);
      for (index = startAt; index < endIdx; index++) {
        if (predicate(list[index])) return true;
      }
      return false;
    }

    public static bool indexWhereOut<A, Data>(
      this IList<A> list, Data data, Func<A, Data, bool> predicate, out int index,
      int startAt = 0, int? count = null
    ) => indexWhereOutC(list, data, predicate, out index, startAt: startAt, count: count);

    public static bool indexWhereOutC<A, Data, Coll>(
      this Coll list, Data data, Func<A, Data, bool> predicate, out int index, 
      int startAt = 0, int? count = null
    ) where Coll : IList<A> {
      var endIdx = startAt + count.GetValueOrDefault(list.Count - startAt);
      for (index = startAt; index < endIdx; index++) {
        if (predicate(list[index], data)) return true;
      }
      return false;
    }

    public static bool average(this IList<float> floats, out float avg) {
      var count = floats.Count;
      if (count == 0) {
        avg = 0;
        return false;
      }
      else {
        var sum = 0f;
        for (var idx = 0; idx < count; idx++) {
          sum += floats[idx];
        }
        avg = sum / count;
        return true;
      }
    }
  }
}
