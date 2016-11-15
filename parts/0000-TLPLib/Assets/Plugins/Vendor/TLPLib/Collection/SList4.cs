using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Collection {
  /* IList<A> like mutable struct which has 4 members in it and falls back 
   * to List<A> if overflown.
   * 
   * Has a benefit of sitting on stack while static size is not exceeded.
   * 
   * Beware that this is a mutable struct - do not store it as a readonly value! Be careful!
   */
  public struct SList4<A> : IList<A> {
    const int STATIC_SIZE = 4;

    A s0, s1, s2, s3;
    List<A> fallback;
    public int size { get; private set; }
    public int Count => size;
    public bool IsReadOnly => false;

    #region Constructors

    public SList4(A a1) {
      size = 1;
      s0 = a1;
      s1 = s2 = s3 = default(A);
      fallback = null;
    }

    public SList4(A a1, A a2) {
      size = 2;
      s0 = a1;
      s1 = a2;
      s2 = s3 = default(A);
      fallback = null;
    }

    public SList4(A a1, A a2, A a3) {
      size = 3;
      s0 = a1;
      s1 = a2;
      s2 = a3;
      s3 = default(A);
      fallback = null;
    }

    public SList4(A a1, A a2, A a3, A a4) {
      size = 4;
      s0 = a1;
      s1 = a2;
      s2 = a3;
      s3 = a4;
      fallback = null;
    }

    public SList4(A a1, A a2, A a3, A a4, params A[] rest) {
      size = 4 + rest.Length;
      s0 = a1;
      s1 = a2;
      s2 = a3;
      s3 = a4;
      fallback = new List<A>(rest);
    }

    #endregion

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
      if (idx < 0 || idx >= size) throw new ArgumentOutOfRangeException(
        $"index {idx} is out of bounds (size: {size})"
      );
    }

    public void clear() {
      s0 = s1 = s2 = s3 = default(A);
      if (fallback != null) fallback.Clear();
      size = 0;
    }
    public void Clear() => clear();

    public void add(A value) {
      size++;
      switch (size) {
        case 1: s0 = value; break;
        case 2: s1 = value; break;
        case 3: s2 = value; break;
        case 4: s3 = value; break;
        default:
          if (fallback == null) {
            if (Log.isVerbose) Log.verbose(
              $"Creating fallback list for {nameof(SList4<A>)}"
            );
            fallback = new List<A>();
          }
          fallback.Add(value);
          break;
      }
    }
    public void Add(A item) => add(item);

    public A removeAt(int idx) {
      checkIndex(idx);

      var value = this[idx];

      if (idx == 0) s0 = s1;
      if (idx <= 1) s1 = s2;
      if (idx <= 2) s2 = s3;
      if (idx <= STATIC_SIZE - 1) {
        if (fallback == null || fallback.Count == 0) s3 = default(A);
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
    public void RemoveAt(int index) => removeAt(index);

    public bool Remove(A item) => IListDefaultImpls.remove(ref this, item);
    public int IndexOf(A item) => IListDefaultImpls.indexOf(this, item);
    public bool Contains(A item) => IListDefaultImpls.contains(this, item);
    public void CopyTo(A[] array, int arrayIndex) => IListDefaultImpls.copyTo(this, array, arrayIndex);
    public void Insert(int index, A item) => IListDefaultImpls.insert(ref this, index, item);

    public override string ToString() => 
      $"{nameof(SList4<A>)}({s0}, {s1}, {s2}, {s3}, {F.opt(fallback).map(_ => _.asString(false)).getOrElse("-")})";

    public IListStructEnumerator<SList4<A>, A> GetEnumerator() => 
      new IListStructEnumerator<SList4<A>, A>(this);
    IEnumerator<A> IEnumerable<A>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  public static class SList4 {
    public static SList4<A> create<A>(A a1) => new SList4<A>(a1);
    public static SList4<A> create<A>(A a1, A a2) => new SList4<A>(a1, a2);
    public static SList4<A> create<A>(A a1, A a2, A a3) => new SList4<A>(a1, a2, a3);
    public static SList4<A> create<A>(A a1, A a2, A a3, A a4) => new SList4<A>(a1, a2, a3, a4);
    public static SList4<A> create<A>(A a1, A a2, A a3, A a4, params A[] rest) => 
      new SList4<A>(a1, a2, a3, a4, rest);
  }
}