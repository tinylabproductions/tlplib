using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Collection {
  /**
   * Carousel like item emitter.
   * 
   * Given:
   * * itemsWithCounts: [("a", 3), ("c", 1), ("b", 2), ("c", 2)]
   * 
   * Will yield:
   * "a", "c", "b", "c", "a", "b", "c", "a", starts from beggining.
   * 
   * This enumerable is endless.
   **/
  public class CarouselEmitter<A> : IEnumerable<A> {
    protected readonly Tpl<A, int>[] itemsWithCounts;
    // Total number of counts. E.g. from [("a", 3), ("b", 1)] this would be 4.
    public readonly int totalCount;
    // Max number of counts. E.g. from [("a", 3), ("b", 1)] this would be 3.
    public readonly int maxCount;

    public CarouselEmitter(
      IEnumerable<Tpl<A, int>> itemsWithCounts
    ) {
      this.itemsWithCounts = itemsWithCounts.Where(t => t._2 > 0).ToArray();

      var totalCnt = 0;
      var maxCnt = 0;
      // ReSharper disable once LoopCanBeConvertedToQuery
      for (var idx = 0; idx < this.itemsWithCounts.Length; idx++) {
        var cnt = this.itemsWithCounts[idx]._2;
        totalCnt += cnt;
        maxCnt = Math.Max(maxCnt, cnt);
      }
      totalCount = totalCnt;
      maxCount = maxCnt;
    }

    public IEnumerator<A> GetEnumerator() {
      if (maxCount <= 0) yield break;

      while (true) {
        for (var idx = 0; idx < maxCount; idx++) {
          foreach (var t in itemsWithCounts.Where(t => t._2 > idx))
            yield return t._1;
        }
      }
    }

    /** Returns an enumerator with N elements skipped. **/
    public IEnumerator<A> GetEnumerator(int skippedElements) {
      var enumerator = GetEnumerator();
      for (var i = 0; i < skippedElements; i++) enumerator.MoveNext();
      return enumerator;
    }

    /** Returns an enumerator which has [0, totalCount) elements skipped. **/
    public IEnumerator<A> GetRandomSkipEnumerator() {
      return GetEnumerator((new Random()).Next(0, totalCount));
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
