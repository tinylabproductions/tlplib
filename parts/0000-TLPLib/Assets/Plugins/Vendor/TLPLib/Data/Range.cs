using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Utilities;
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
  }

  [Serializable]
  // [from, to]
  public struct Range {
    // No it can't, Unity...
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [SerializeField] int _from, _to;
    public int from => _from;
    public int to => _to;

    public Range(int from, int to) {
      _from = from;
      _to = to;
    }

    public int random { get { return Random.Range(from, to + 1); } }

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
    public readonly uint from, to;

    public URange(uint from, uint to) {
      this.from = from;
      this.to = to;
    }

    public uint random { get { return (uint) Random.Range(from, to + 1); } }
  }

  [Serializable]
  public struct FRange {
    // No it can't, Unity...
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    [SerializeField] private float _from, _to;
    public float from { get { return _from; } }
    public float to { get { return _to; } }

    public FRange(float from, float to) {
      _from = from;
      _to = to;
    }

    public float random { get { return Rng.range(from, to); } }

    public EnumerableFRange by(float step) {
      return new EnumerableFRange(from, to, step);
    }

    public override string ToString() {
      return string.Format("({0} to {1})", from, to);
    }
  }

  public struct EnumerableFRange : IEnumerable<float> {
    public readonly float from, to, step;

    public EnumerableFRange(float from, float to, float step) {
      this.from = from;
      this.to = to;
      this.step = step;
    }

    public float random { get { return Rng.range(from, to); } }

    public IEnumerator<float> GetEnumerator() {
      for (var i = from; i <= to; i += step)
        yield return i;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public override string ToString() {
      return string.Format("({0} to {1} by {2})", from, to, step);
    }
  }
}
