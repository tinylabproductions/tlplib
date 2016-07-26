using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class ArrayUtils {
    static readonly Dictionary<Type, object> emptyArrays = 
      new Dictionary<Type, object>();

    /** 
     * When you have a serialized array in Unity if the asset was not opened
     * in editor and reserialized, runtime will return null for that array 
     * instead of empty array. Yay Unity!
     */
    public static A[] ensureNotNull<A>(this A[] array) =>
      array ?? (A[]) emptyArrays.getOrUpdate(typeof (A), () => new A[0]);
  }
}