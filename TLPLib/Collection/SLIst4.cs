using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Collection {
  /* IList<A> like mutable struct which has 4 members in it and falls back 
   * to IList<A> if overflown.
   * 
   * Has a benefit of sitting on stack while static size is not exceeded.
   * 
   * Beware that this is a mutable struct - do not store it as a readonly value! Be careful!
   */
  public struct SList4<A> {
    const int STATIC_SIZE = 4;

    A s0, s1, s2, s3;
    List<A> fallback;
    public int size { get; private set; }

    public A this[int idx] { 
      get {
        checkIndex(idx);
        switch (idx) {
          case 0: return s0;
          case 1: return s1;
          case 2: return s2;
          case 3: return s3;
          default: return fallback[idx - STATIC_SIZE];
        }
      }
      set {
        checkIndex(idx);
        switch (idx) {
          case 0: s0 = value; break;
          case 1: s1 = value; break;
          case 2: s2 = value; break;
          case 3: s3 = value; break;
          default: fallback[idx - STATIC_SIZE] = value; break;
        }
      }
    }

    void checkIndex(int idx) {
      if (idx < 0 || idx >= size) throw new IndexOutOfRangeException(string.Format(
        "index {0} is out of bounds (size: {1})", idx, size
      ));
    }

    public void clear() {
      s0 = s1 = s2 = s3 = default(A);
      if (fallback != null) fallback.Clear();
      size = 0;
    }

    public void add(A value) {
      size++;
      switch (size) {
        case 1: s0 = value; break;
        case 2: s1 = value; break;
        case 3: s2 = value; break;
        case 4: s3 = value; break;
        default:
          if (fallback == null) {
            Log.debug("Creating fallback list for SList4");
            fallback = new List<A>();
          }
          fallback.Add(value);
          break;
      }
    }

    public A removeAt(int idx) {
      checkIndex(idx);

      var value = this[idx];

      if (idx == 0) s0 = s1;
      if (idx <= 1) s1 = s2;
      if (idx <= 2) s2 = s3;
      if (idx <= STATIC_SIZE - 1) {
        if (fallback == null || fallback.Count > 0) s3 = default(A);
        else {
          s3 = fallback[0];
          fallback.RemoveAt(0);
        }
      }
      // ReSharper disable once PossibleNullReferenceException
      if (idx > STATIC_SIZE - 1) fallback.RemoveAt(idx - STATIC_SIZE);

      size--;

      return value;
    }

    public string asString() {
      return string.Format(
        "SList8({0}, {1}, {2}, {3}, {4})",
        s0, s1, s2, s3,
        F.opt(fallback).map(_ => _.asString(false)).getOrElse("-")
      );
    }
  }
}
