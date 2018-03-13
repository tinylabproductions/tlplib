using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.serialization;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional {
  public struct Unit : IEquatable<Unit> {
    public static Unit instance { get; } = new Unit();
    public override string ToString() => "()";
    
    [PublicAPI] public static ISerializedRW<Unit> rw => UnitRW.instance;

    #region Equality

    public bool Equals(Unit other) => true;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Unit unit && Equals(unit);
    }

    public override int GetHashCode() => nameof(Unit).GetHashCode();

    public static bool operator ==(Unit left, Unit right) { return left.Equals(right); }
    public static bool operator !=(Unit left, Unit right) { return !left.Equals(right); }

    #endregion
  }
}
