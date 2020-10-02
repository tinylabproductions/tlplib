using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using pzd.lib.concurrent;
using pzd.lib.dispose;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class CoroutineHelpers {
    public static AggregateCoroutine playCoroutines(params IEnumerator[] a) =>
      a.Select(ASync.StartCoroutine).aggregate();

    public static AggregateCoroutine playAllWithDelay<A>(
      IEnumerable<A> items, Func<A, int, IEnumerator> func, Duration? delayBetween = null, bool unscaledTime = false,
      Duration? delayBeforeAll = null, Func<bool> continuePlaying = null
    ) => playAllWithDelay(
      items.Select(func), delayBetween, unscaledTime, delayBeforeAll, continuePlaying: continuePlaying
    );

    public static IEnumerator durationPercentage(Duration d, Action<Percentage> act) {
      foreach (var p in new CoroutineInterval(d)) {
        act(p);
        yield return null;
      }
    }

    public static IEnumerator withDelayUnscaled(IEnumerator enumeratorNext, float seconds) {
      yield return new WaitForSecondsUnscaled(seconds);
      yield return enumeratorNext;
    }

    public static IEnumerator withDelayNormal(IEnumerator enumeratorNext, float seconds, Func<bool> continuePlaying) {
      yield return new WaitWithCancel(seconds, continuePlaying);
      yield return enumeratorNext;
    }

    public static AggregateCoroutine playAllWithDelay(
      IEnumerable<IEnumerator> items, Duration? delayBetween = null, bool unscaledTime = false,
      Duration? delayBeforeAll = null, Func<bool> continuePlaying = null
    ) {
      // known issue: continuePlaying does nothing with unscaledTime
      continuePlaying ??= () => true;
      return items.Select((_, i) => ASync.StartCoroutine(
        unscaledTime
          ? withDelayUnscaled(_, calcDelay(i))
          : withDelayNormal(_, calcDelay(i), continuePlaying)
      )).aggregate();

      float calcDelay(int index) => delayBetween.GetValueOrDefault(Duration.zero).seconds * index
        + delayBeforeAll.GetValueOrDefault(Duration.zero).seconds;
    }

    public static IEnumerator playOneByOne<A>(
      A[] initsAll, Func<A, IEnumerator> func, Duration? delayBetween = null,
      Action onUpdate = null, bool unscaledTime = false, Func<bool> continuePlaying = null
    ) => playOneByOne(initsAll.Select(func), delayBetween, onUpdate, unscaledTime);

    public static IEnumerator playOneByOne(
      IEnumerable<IEnumerator> enums, Duration? delayBetween = null, Action onUpdate = null, bool unscaledTime = false,
      Func<bool> continuePlaying = null
    ) {
      continuePlaying = continuePlaying ?? (() => true);
      foreach (var e in enums) {
        while (e.MoveNext()) {
          onUpdate?.Invoke();
          yield return e.Current;
        }
        if (delayBetween.HasValue) {
          yield return new WaitWithCancel(delayBetween.Value.seconds, continuePlaying, unscaledTime: unscaledTime);
        }
      }
    }
  }
}