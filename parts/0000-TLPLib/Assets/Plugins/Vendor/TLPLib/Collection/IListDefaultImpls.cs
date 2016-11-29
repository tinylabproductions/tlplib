using System;
using System.Collections.Generic;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Collection {
  public static class IListDefaultImpls {
    public static int indexOf<A, C>(C c, A item) where C : IList<A> {
      for (var idx = 0; idx < c.Count; idx++)
        if (EqComparer<A>.Default.Equals(c[idx], item))
          return idx;
      return -1;
    }

    public static bool contains<A, C>(C c, A item) where C : IList<A> =>
      indexOf(c, item) != -1;

    public static bool remove<A, C>(ref C c, A item) where C : IList<A> {
      for (var idx = 0; idx < c.Count; idx++)
        if (EqComparer<A>.Default.Equals(c[idx], item)) {
          c.RemoveAt(idx);
          return true;
        }
      return false;
    }

    public static void insert<A, C>(ref C c, int idx, A item) where C : IList<A> {
      if (idx > c.Count) throw new ArgumentOutOfRangeException(
        nameof(idx), idx, "index is greater than the list size"
      );
      if (idx < 0) throw new ArgumentOutOfRangeException(
        nameof(idx), idx, "index is lesser than 0"
      );

      if (idx == c.Count) c.Add(item);
      else {
        var lastIdx = c.Count - 1;
        c.Add(c[lastIdx]);
        for (var i = lastIdx; i > idx; i--) c[i] = c[i - 1];
        c[idx] = item;
      }
    }
    
    public static void copyTo<C, A>(
      C c, A[] array, int targetStartIndex, int srcCopyFrom = 0, int srcCopyCount = -1
    )
      where C : IList<A>
    {
      if (array == null)
        throw new ArgumentNullException(nameof(array), "array is null");
      if (targetStartIndex < 0)
        throw new ArgumentOutOfRangeException(
          nameof(targetStartIndex), 
          $"array index ({targetStartIndex}) is < 0"
        );
      if (srcCopyFrom < 0)
        throw new ArgumentOutOfRangeException(
          nameof(srcCopyFrom), 
          $"array index ({srcCopyFrom}) is < 0"
        );
      if (srcCopyFrom >= c.Count)
        throw new ArgumentOutOfRangeException(
          nameof(srcCopyFrom), 
          $"array index ({srcCopyFrom}) is > collection count ({c.Count})"
        );
      if (srcCopyCount < 0) srcCopyCount = c.Count - srcCopyFrom;
      var targetEndIndex = targetStartIndex + srcCopyCount;
      if (array.Length < targetEndIndex) throw new ArgumentException(
        $"Target array is too small ({nameof(targetEndIndex)}={targetEndIndex}, " +
        $"array length={array.Length})"
      );

      for (
        int srcIdx = srcCopyFrom, targetIdx = targetStartIndex;
        targetIdx < targetEndIndex; 
        srcIdx++, targetIdx++
      ) {
        array[targetIdx] = c[srcIdx];
      }
    }
  }
}