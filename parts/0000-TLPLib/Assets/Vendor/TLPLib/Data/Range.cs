using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Data {
  public static class RangeExts {
    [PublicAPI] public static Range to(this int from, int to) => new Range(from, to);
    [PublicAPI] public static Range until(this int from, int to) => new Range(@from, to - 1);
    [PublicAPI] public static float lerpVal(this FRange range, float t) => Mathf.Lerp(range.from, range.to, t);
    [PublicAPI] public static float lerpVal(this Range range, float t) => Mathf.Lerp(range.from, range.to, t);
  }

  /// <summary>Integer range: [from, to].</summary>
  [Serializable]
  public struct Range : IEnumerable<int>, OnObjectValidate {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] int _from;
    [SerializeField, Tooltip("Inclusive")] int _to;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public int from => _from;
    public int to => _to;

    public Range(int from, int to) {
      _from = from;
      _to = to;
    }

    public bool inRange(int v) =>  from <= v && v <= to; 
    public int random => Random.Range(from, to + 1);
    public int this[Percentage p] => from + (int) ((to - from) * p.value);
    public override string ToString() => $"({from} to {to})";
    
    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (_from > _to) yield return new ErrorMsg(
        $"Expected from ({_from}) to be <= to ({_to})"
      );
    }

    [PublicAPI] public RangeEnumerator GetEnumerator() => new RangeEnumerator(from, to);
    IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  public struct RangeEnumerator : IEnumerator<int> {
    public readonly int start, end;
    bool firstElement;

    public int Current { get; set; }

    public RangeEnumerator(int start, int end) {
      this.start = start;
      this.end = end;
      firstElement = false;
      Current = 0;
      Reset();
    }

    public bool MoveNext() {
      if (firstElement && Current <= end) {
        firstElement = false;
        return true;
      }
      if (Current == end) return false;
      Current++;
      return Current <= end;
    }

    public void Reset() {
      firstElement = true;
      Current = start;
    }

    object IEnumerator.Current => Current;
    public void Dispose() {}
  }

  [Serializable, PublicAPI] 
  public struct URange : IEnumerable<uint>, OnObjectValidate {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] uint _from;
    [SerializeField, Tooltip("Inclusive")] uint _to;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public uint from => _from;
    public uint to => _to;

    public URange(uint from, uint to) {
      _from = from;
      _to = to;
    }

    // https://stackoverflow.com/a/3269471/935259
    public bool overlaps(URange o) => _from <= o._to && o._from <= _to;
    
    public uint random => (uint) Random.Range(from, to + 1);
    public uint this[Percentage p] => from + (uint) ((to - from) * p.value);
    public override string ToString() => $"({from} to {to})";
    
    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (_from > _to) yield return new ErrorMsg(
        $"Expected from ({_from}) to be <= to ({_to})"
      );
    }

    public URangeEnumerator GetEnumerator() => new URangeEnumerator(from, to);
    IEnumerator<uint> IEnumerable<uint>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public static URange operator +(URange r, uint i) => new URange(from: r._from + i, to: r._to + i);
  }

  public struct URangeEnumerator : IEnumerator<uint> {
    public readonly uint start, end;
    bool firstElement;

    public uint Current { get; set; }

    public URangeEnumerator(uint start, uint end) {
      this.start = start;
      this.end = end;
      firstElement = false;
      Current = 0;
      Reset();
    }

    public bool MoveNext() {
      if (firstElement && Current <= end) {
        firstElement = false;
        return true;
      }
      if (Current == end) return false;
      Current++;
      return Current <= end;
    }

    public void Reset() {
      firstElement = true;
      Current = start;
    }

    object IEnumerator.Current => Current;
    public void Dispose() { }
  }

  [Serializable]
  public struct FRange : OnObjectValidate {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] float _from;
    [SerializeField, Tooltip("Inclusive")] float _to;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public float from => _from;
    public float to => _to;

    public FRange(float from, float to) {
      _from = from;
      _to = to;
    }

    [PublicAPI] public float random => Random.Range(from, to);
    [PublicAPI] public float this[Percentage p] => from + (to - from) * p.value;
    [PublicAPI] public bool contains(float f) => f >= _from && f <= _to;
    [PublicAPI] public float diff => _to - _from;
    [PublicAPI] public float at(float percentage) => from + diff * percentage;

    public override string ToString() => $"({from} to {to})";

    [PublicAPI] public EnumerableFRange by(float step) => new EnumerableFRange(@from, to, step);

    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (_from > _to) yield return new ErrorMsg(
        $"Expected from ({_from}) to be <= to ({_to})"
      );
    }
  }

  [Serializable]
  public struct EnumerableFRange : IEnumerable<float>, OnObjectValidate {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] float _from;
    [SerializeField, Tooltip("Inclusive")] float _to;
    [SerializeField] float _step;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public float from => _from;
    public float to => _to;
    public float step => _step;

    public EnumerableFRange(float from, float to, float step) {
      _from = from;
      _to = to;
      _step = step;
    }

    public float random => Random.Range(from, to);
    public float this[Percentage p] => from + (to - from) * p.value;
    public override string ToString() => $"({from} to {to} by {step})";

    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
      if (_from > _to) yield return new ErrorMsg(
        $"Expected from ({_from}) to be <= to ({_to})"
      );
    }

    public IEnumerator<float> GetEnumerator() {
      for (var i = from; i <= to; i += step)
        yield return i;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
