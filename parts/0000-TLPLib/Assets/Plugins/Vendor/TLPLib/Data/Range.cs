using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Data {
  public static class RangeExts {
    public static Range to(this int from, int to) {
      return new Range(from, to);
    }
    public static Range until(this int from, int to) {
      return new Range(from, to - 1);
    }

    public static float lerpVal(this FRange range, float t) => Mathf.Lerp(range.from, range.to, t);
    public static float lerpVal(this Range range, float t) => Mathf.Lerp(range.from, range.to, t);
  }

  [Serializable]
  public struct Range {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] int _from, _to;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public int from => _from;
    public int to => _to;

    public Range(int from, int to) {
      _from = from;
      _to = to;
    }

    public int random => Random.Range(from, to + 1);
    public int this[Percentage p] => from + (int) ((to - from) * p.value);
    public override string ToString() => $"({from} to {to})";

    public RangeEnumerator GetEnumerator() { return new RangeEnumerator(from, to); }
  }

  public struct RangeEnumerator {
    public readonly int start, end;
    bool firstElement;

    public RangeEnumerator(int start, int end) {
      this.start = start;
      this.end = end;
      firstElement = default(bool);
      Current = default(int);
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

    public int Current { get; set; }
  }
  
  [Serializable]
  public struct URange {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] uint _from, _to;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public uint from => _from;
    public uint to => _to;

    public URange(uint from, uint to) {
      _from = from;
      _to = to;
    }

    public uint random => (uint) Random.Range(from, to + 1);
    public uint this[Percentage p] => from + (uint) ((to - from) * p.value);
    public override string ToString() => $"({from} to {to})";
  }

  [Serializable]
  public struct FRange {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] float _from, _to;
#pragma warning restore 649
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    #endregion

    public float from => _from;
    public float to => _to;

    public FRange(float from, float to) {
      _from = from;
      _to = to;
    }

    public float random => Random.Range(from, to);
    public float this[Percentage p] => from + (to - from) * p.value;
    public override string ToString() => $"({from} to {to})";

    public EnumerableFRange by(float step) => new EnumerableFRange(@from, to, step);
  }

  [Serializable]
  public struct EnumerableFRange : IEnumerable<float> {
    #region Unity Serialized Fields
    // ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649
    [SerializeField] float _from, _to, _step;
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

    public IEnumerator<float> GetEnumerator() {
      for (var i = from; i <= to; i += step)
        yield return i;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
