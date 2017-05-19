using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// Implementation of XORSHIFT, random number generation algorithm unity uses for Random.
  /// 
  /// https://forum.unity3d.com/threads/which-random-number-generator-does-unity-use.148601/
  /// https://en.wikipedia.org/wiki/Xorshift
  /// </summary>
  public struct Rng {
    readonly uint s0, s1, s2, s3;

    /* The state array must be initialized to not be all zero */
    Rng(uint s0, uint s1, uint s2, uint s3) {
      this.s0 = s0;
      this.s1 = s1;
      this.s2 = s2;
      this.s3 = s3;
    }

    public Rng(DateTime seed) : this(
      unchecked((uint) seed.Millisecond),
      unchecked((uint) seed.Second),
      unchecked((uint) seed.DayOfYear),
      unchecked((uint) seed.Year)
    ) {}

    public override string ToString() => $"{nameof(Rng)}[{s0}, {s1}, {s2}, {s3}]";

    public Tpl<Rng, uint> nextUIntT { get {
      Rng newState;
      var res = nextUInt(out newState);
      return F.t(newState, res);
    } }
    public static readonly Fn<Rng, Tpl<Rng, uint>> nextUIntS = rng => rng.nextUIntT;

    public uint nextUInt(out Rng newState) {
      var t = s3;
      t ^= t << 11;
      t ^= t >> 8;
      t ^= s0;
      t ^= s0 >> 19;
      newState = new Rng(t, s0, s1, s2);
      return t;
    }

    const uint HALF_OF_UINT = uint.MaxValue / 2;
    static bool uintToBool(uint v) => v >= HALF_OF_UINT;
    public Tpl<Rng, bool> nextBoolT => nextUIntT.map2(uintToBool);
    public static readonly Fn<Rng, Tpl<Rng, bool>> nextBoolS = rng => rng.nextBoolT;
    public bool nextBool(out Rng newState) => uintToBool(nextUInt(out newState));

    static int uintToInt(uint v) => unchecked((int) v);
    public Tpl<Rng, int> nextIntT => nextUIntT.map2(uintToInt);
    public static readonly Fn<Rng, Tpl<Rng, int>> nextIntS = rng => rng.nextIntT;
    public int nextInt(out Rng newState) => uintToInt(nextUInt(out newState));

    static float uintToFloat(uint v) => (float) v / uint.MaxValue;
    public Tpl<Rng, float> nextFloatT => nextUIntT.map2(uintToFloat);
    public static readonly Fn<Rng, Tpl<Rng, float>> nextFloatS = rng => rng.nextFloatT;
    public float nextFloat(out Rng newState) => uintToFloat(nextUInt(out newState));

    static int floatToIntInRange(Range range, float v) => 
      range.from + (int)((range.to - range.from) * v);
    public Tpl<Rng, int> nextIntInRangeT(Range range) =>
      nextFloatT.map2(v => floatToIntInRange(range, v));
    public static Fn<Rng, Tpl<Rng, int>> nextIntInRangeS(Range range) => 
      rng => rng.nextIntInRangeT(range);
    public int nextIntInRange(Range range, out Rng newState) => 
      floatToIntInRange(range, nextFloat(out newState));

    static float floatToFloatInRange(FRange range, float v) => 
      range.from + (range.to - range.from) * v;
    public Tpl<Rng, float> nextFloatInRangeT(FRange range) =>
      nextFloatT.map2(v => floatToFloatInRange(range, v));
    public static Fn<Rng, Tpl<Rng, float>> nextFloatInRangeS(FRange range) =>
      rng => rng.nextFloatInRangeT(range);
    public float nextFloatInRange(FRange range, out Rng newState) =>
      floatToFloatInRange(range, nextFloat(out newState));
  }
}