using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  class UnitRW : ISerializedRW<Unit> {
    public static readonly UnitRW instance = new UnitRW();
    UnitRW() {}

    static readonly Rope<byte> UNIT_ROPE = Rope.a(new byte[0]);
    static readonly Either<string, DeserializeInfo<Unit>> DESERIALIZE_INFO =
      new DeserializeInfo<Unit>(F.unit, 0);

    public Rope<byte> serialize(Unit a) => UNIT_ROPE;
    public Either<string, DeserializeInfo<Unit>> deserialize(byte[] serialized, int startIndex) =>
      DESERIALIZE_INFO;
  }
}