﻿using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ArrayExts {
    /** Copy the array, adding the element. */
    public static A[] addOne<A>(this A[] arr, A a) {
      var newArr = new A[arr.LongLength + 1];
      arr.CopyTo(newArr, 0);
      newArr[arr.LongLength] = a;
      return newArr;
    }

    public static A[] concat<A>(this A[] a, A[] other) {
      var newArr = new A[a.LongLength + other.LongLength];
      a.CopyTo(newArr, 0);
      other.CopyTo(newArr, a.LongLength);
      return newArr;
    }

    public static A[] concat<A>(this A[] a, params A[][] others) {
      // Functional programming crashes Mono runtime.

      var total = 0;
      // ReSharper disable once LoopCanBeConvertedToQuery
      // Mono runtime bug.
      for (var idx = 0; idx < others.Length; idx++)
        total += others[idx].Length;

      var self = new A[a.Length + total];
      a.CopyTo(self, 0);
      var startIdx = a.Length;
      // ReSharper disable once ForCanBeConvertedToForeach
      // Mono runtime bug.
      for (var idx = 0; idx < others.Length; idx++) {
        var arr = others[idx];
        arr.CopyTo(self, startIdx);
        startIdx += arr.Length;
      }

      return self;
    }

    public static ImmutableArray<To> toImmutable<From, To>(
      this From[] source, Func<From, To> mapper
    ) {
      var b = ImmutableArray.CreateBuilder<To>(source.Length);
      foreach (var a in source) b.Add(mapper(a));
      return b.MoveToImmutable();
    }
    
    public static A[] removeAt<A>(this A[] source, int index) {
      if (index < 0 || index >= source.Length) throw new ArgumentOutOfRangeException(
        nameof(index), index, $"index out of range, length = {source.Length}"
      );
      var dest = new A[source.Length - 1];
      if (index > 0)
        Array.Copy(source, 0, dest, 0, index);

      if (index < source.Length - 1)
        Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

      return dest;
    }

    /// <summary>
    /// Removes specified index, shifting everything to left and replaces last element with given value.
    /// </summary>
    public static void removeAt<A>(this A[] arr, int idx, A replaceLastElementWith) {
      for (var i = idx; i < arr.Length - 1; i++) {
        arr[i] = arr[i + 1];
      }
      arr[arr.Length - 1] = replaceLastElementWith;
    }

    public static bool contains<A>(this A[] arr, A a) => Array.IndexOf(arr, a) != -1;

    [PublicAPI]
    public static void fill<A>(this A[] arr, A a) {
      for (var idx = 0; idx < arr.Length; idx++)
        arr[idx] = a;
    }

    [PublicAPI]
    public static A[] clone<A>(this A[] arr) {
      var newArr = new A[arr.LongLength];
      for (var idx = 0L; idx < arr.LongLength; idx++) {
        newArr[idx] = arr[idx];
      }
      return newArr;
    }

    /// <summary>Moves all values in array to the left by given offset.</summary>
    ///
    /// <example>
    /// var arr = new []{1,2,3,4,5};
    /// arr.shiftLeft(2);
    /// // arr now is {3,4,5,4,5};
    /// </example>
    [PublicAPI]
    public static void shiftLeft<A>(this A[] arr, uint shiftBy) {
      if (shiftBy >= arr.LongLength) throw new ArgumentException(
        $"Can't shift array elements by {shiftBy} when array size is {arr.LongLength}!",
        nameof(shiftBy)
      );
      
      for (var idx = 0u; idx < arr.LongLength - shiftBy; idx++) {
        arr[idx] = arr[idx + shiftBy];
      }
    }

    [PublicAPI]
    public static void setValues<A>(this A[] target, A[] source) {
      if (target.LongLength != source.LongLength) throw new ArgumentException(
        $"target size {target.LongLength} does not match source size {source.LongLength}"
      );

      for (var idx = 0L; idx < target.LongLength; idx++) {
        target[idx] = source[idx];
      }
    }
  }
}
