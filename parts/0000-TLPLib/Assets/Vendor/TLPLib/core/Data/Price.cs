using System.Collections.Generic;
using GenerationAttributes;
using pzd.lib.typeclasses;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Record(GenerateToString = false)] public readonly partial struct Price {
    public readonly int cents;
    
    public override string ToString() => $"{nameof(Price)}({cents * 0.01})";

    [HideInInspector] public static readonly Numeric<Price> numeric = new Numeric();
    public static IEqualityComparer<Price> eql => numeric;
    public static Comparable<Price> comparable => numeric;

    class Numeric : Numeric<Price> {
      public Price add(Price a1, Price a2) => new Price(a1.cents + a2.cents);
      public Price subtract(Price a1, Price a2) => new Price(a1.cents - a2.cents);
      public Price mult(Price a1, Price a2) => new Price(a1.cents * a2.cents);
      public Price div(Price a1, Price a2) => new Price(a1.cents / a2.cents);
      public Price fromInt(int i) => new Price(i);
      public bool Equals(Price a1, Price a2) => a1 == a2;
      public int GetHashCode(Price obj) => obj.GetHashCode();
      public CompareResult compare(Price a1, Price a2) => Compare(a1, a2).asCmpRes();
      public int Compare(Price x, Price y) => x.cents.CompareTo(y.cents);
    }
  }
}