﻿using System;
using pzd.lib.typeclasses;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Price : IEquatable<Price> {
    public readonly int cents;

    public Price(int cents) { this.cents = cents; }

    #region Equality

    public bool Equals(Price other) {
      return cents == other.cents;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Price && Equals((Price) obj);
    }

    public override int GetHashCode() {
      return cents;
    }

    public static bool operator ==(Price left, Price right) { return left.Equals(right); }
    public static bool operator !=(Price left, Price right) { return !left.Equals(right); }

    #endregion

    public override string ToString() => $"{nameof(Price)}({cents * 0.01})";

    [HideInInspector] public static readonly Numeric<Price> numeric = new Numeric();
    public static Eql<Price> eql => numeric;
    public static Comparable<Price> comparable => numeric;

    class Numeric : Numeric<Price> {
      public Price add(Price a1, Price a2) => new Price(a1.cents + a2.cents);
      public Price subtract(Price a1, Price a2) => new Price(a1.cents - a2.cents);
      public Price mult(Price a1, Price a2) => new Price(a1.cents * a2.cents);
      public Price div(Price a1, Price a2) => new Price(a1.cents / a2.cents);
      public Price fromInt(int i) => new Price(i);
      public bool eql(Price a1, Price a2) => a1 == a2;
      public CompareResult compare(Price a1, Price a2) => Compare(a1, a2).asCmpRes();
      public int Compare(Price x, Price y) => x.cents.CompareTo(y.cents);
    }
  }
}