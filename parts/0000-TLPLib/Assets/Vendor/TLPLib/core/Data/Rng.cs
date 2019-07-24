using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// Implementation of XORSHIFT, random number generation algorithm unity uses for Random.
  ///
  /// https://forum.unity3d.com/threads/which-random-number-generator-does-unity-use.148601/
  /// https://en.wikipedia.org/wiki/Xorshift
  ///
  /// This implementation uses xorshift* version.
  /// </summary>
  public class Rng {
    public class Seed {
      public readonly ulong seed;

      public Seed(ulong seed) {
        // XORSHIFT does not work with 0 seeds.
        if (seed == 0) throw new ArgumentOutOfRangeException(nameof(seed), seed, "seed can't be 0!");
        this.seed = seed;
      }

      public bool isInitialized => seed != 0;

      public override string ToString() => $"{nameof(Seed)}({seed})";
    }

    public readonly Seed seed;

    public static Seed seedFrom(DateTime dt) => new Seed(unchecked((ulong) dt.Ticks));
    public static Seed nowSeed => seedFrom(DateTime.Now);
    public static Rng now => new Rng(nowSeed);

    public Rng(Seed seed) {
      this.seed = seed;
    }

    public Rng(DateTime seed) : this(seedFrom(seed)) {}

    public bool isInitialized => seed.isInitialized;

    public override string ToString() => $"{nameof(Rng)}({seed})";

    public Tpl<Rng, ulong> nextULongT { get {
      Rng newState;
      var res = nextULong(out newState);
      return F.t(newState, res);
    } }
    public static readonly Func<Rng, Tpl<Rng, ulong>> nextULongS = rng => rng.nextULongT;

    public ulong nextULong(out Rng newState) {
      var x = seed.seed;
      x ^= x >> 12; // a
      x ^= x << 25; // b
      x ^= x >> 27; // c
      newState = new Rng(new Seed(x));
      return x * 0x2545F4914F6CDD1D;
    }

    #region bool

    const ulong HALF_OF_ULONG = ulong.MaxValue / 2;
    static bool ulongToBool(ulong v) => v >= HALF_OF_ULONG;
    public Tpl<Rng, bool> nextBoolT => nextULongT.map2(ulongToBool);
    public static readonly Func<Rng, Tpl<Rng, bool>> nextBoolS = rng => rng.nextBoolT;
    public bool nextBool(out Rng newState) => ulongToBool(nextULong(out newState));

    #endregion

    #region int

    static int ulongToInt(ulong v) => unchecked((int)v);
    public Tpl<Rng, int> nextIntT => nextULongT.map2(ulongToInt);
    public static readonly Func<Rng, Tpl<Rng, int>> nextIntS = rng => rng.nextIntT;
    public int nextInt(out Rng newState) => ulongToInt(nextULong(out newState));

    static int ulongToIntInRange(Range range, ulong v) =>
      range.from + (int) (v % (ulong) (range.to - range.from + 1));
    public Tpl<Rng, int> nextIntInRangeT(Range range) =>
      nextULongT.map2(v => ulongToIntInRange(range, v));
    public static Func<Rng, Tpl<Rng, int>> nextIntInRangeS(Range range) =>
      rng => rng.nextIntInRangeT(range);
    public int nextIntInRange(Range range, out Rng newState) =>
      ulongToIntInRange(range, nextULong(out newState));

    #endregion

    #region uint

    static uint ulongToUInt(ulong v) => unchecked((uint)v);
    public Tpl<Rng, uint> nextUIntT => nextULongT.map2(ulongToUInt);
    public static readonly Func<Rng, Tpl<Rng, uint>> nextUIntS = rng => rng.nextUIntT;
    public uint nextUInt(out Rng newState) => ulongToUInt(nextULong(out newState));

    static uint ulongToUIntInRange(URange range, ulong v) =>
      range.from + (uint)(v % (range.to - range.from + 1));
    public Tpl<Rng, uint> nextUIntInRangeT(URange range) =>
      nextUIntT.map2(v => ulongToUIntInRange(range, v));
    public static Func<Rng, Tpl<Rng, uint>> nextUIntInRangeS(URange range) =>
      rng => rng.nextUIntInRangeT(range);
    public uint nextUIntInRange(URange range, out Rng newState) =>
      ulongToUIntInRange(range, nextULong(out newState));

    #endregion

    #region float

    static float ulongToFloat(ulong v) => (float)v / ulong.MaxValue;
    public Tpl<Rng, float> nextFloatT => nextULongT.map2(ulongToFloat);
    public static readonly Func<Rng, Tpl<Rng, float>> nextFloatS = rng => rng.nextFloatT;
    /// <returns>0f to 1f</returns>
    public float nextFloat(out Rng newState) => ulongToFloat(nextULong(out newState));

    static float floatToFloatInRange(FRange range, float v) =>
      range.from + (range.to - range.from) * v;
    public Tpl<Rng, float> nextFloatInRangeT(FRange range) =>
      nextFloatT.map2(v => floatToFloatInRange(range, v));
    public static Func<Rng, Tpl<Rng, float>> nextFloatInRangeS(FRange range) =>
      rng => rng.nextFloatInRangeT(range);
    public float nextFloatInRange(FRange range, out Rng newState) =>
      floatToFloatInRange(range, nextFloat(out newState));

    #endregion
  }
}