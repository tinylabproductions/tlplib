using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.serialization;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional {
  [Serializable]
  public struct Unit : IEquatable<Unit> {
    public static Unit instance { get; } = new Unit();
    public override string ToString() => "()";
    
    [PublicAPI] public static ISerializedRW<Unit> rw => UnitRW.instance;

    #region Equality

    public bool Equals(Unit other) => true;

    public override bool Equals(object obj) => obj is Unit;

    public override int GetHashCode() => 848053388; // just random numbers

    public static bool operator ==(Unit left, Unit right) => left.Equals(right);
    public static bool operator !=(Unit left, Unit right) => !left.Equals(right);

    #endregion
  }
}
