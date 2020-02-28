using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using pzd.lib.collection;
using pzd.lib.data;
using pzd.lib.functional;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class IReadOnlyCollectionExts {
    public static Option<int> randomIndex<A>(this IReadOnlyCollection<A> list) =>
      list.Count == 0 
        ? F.none<int>() 
        : F.some(Random.Range(0, list.Count));

    public static Option<int> randomIndex<A>(this IReadOnlyCollection<A> list, ref Rng rng) =>
      list.Count == 0
        ? F.none<int>()
        : F.some(rng.nextIntInRange(0, list.Count - 1, out rng));

    public static Option<Tpl<Rng, int>> randomIndexT<A>(this IReadOnlyCollection<A> list, Rng rng) =>
      list.Count == 0
        ? F.none<Tpl<Rng, int>>()
        : F.some(rng.nextIntInRangeT(0, list.Count - 1).toTpl());
    
    public static Option<A> random<A>(this IReadOnlyList<A> list) =>
      list.randomIndex().map(list, (idx, l) => l[idx]);

    public static Option<A> random<A>(this IReadOnlyList<A> list, ref Rng rng) =>
      list.randomIndex(ref rng).map(list, (idx, l) => l[idx]);

    public static Option<Tpl<Rng, A>> randomT<A>(this IReadOnlyList<A> list, Rng rng) =>
      list.randomIndexT(rng).map(list, (t, l) => F.t(t._1, l[t._2]));

    public static A random<C, A>(this NonEmpty<C> list, ref Rng rng) where C : IReadOnlyList<A> =>
      list.a[rng.nextIntInRange(0, list.a.Count - 1, out rng)];

    public static A random<A>(this NonEmpty<ImmutableList<A>> list, ref Rng rng) =>
      random<ImmutableList<A>, A>(list, ref rng);

    public static A random<A>(this NonEmpty<ImmutableArray<A>> list, ref Rng rng) =>
      random<ImmutableArray<A>, A>(list, ref rng);

    public static A random<A>(this NonEmpty<ImmutableArrayC<A>> list, ref Rng rng) =>
      random<ImmutableArrayC<A>, A>(list, ref rng);
    
    /// <summary>
    /// Returns a random element. The probability is selected by element weight.
    /// </summary>
    public static Option<A> randomElementByWeight<A>(
      this IReadOnlyList<A> list, Func<A, float> weightSelector, ref Rng rng
    ) {
      if (list.Count == 0) return F.none<A>();

      var totalWeight = 0f;
      // ReSharper disable once LoopCanBeConvertedToQuery
      for (var idx = 0; idx < list.Count; idx++)
        totalWeight += weightSelector(list[idx]);

      // The weight we are after...
      var itemWeightIndex = rng.nextFloat(out rng) * totalWeight;
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
  }
}