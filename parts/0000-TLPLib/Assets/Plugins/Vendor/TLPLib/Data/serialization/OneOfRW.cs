using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  static class OneOfRW {
    public const byte
      DISCRIMINATOR_A = (byte) 'a',
      DISCRIMINATOR_B = (byte) 'b',
      DISCRIMINATOR_C = (byte) 'c';

    public static readonly Rope<byte>
      DISCRIMINATOR_A_ROPE = Rope.a(new[] { DISCRIMINATOR_A }),
      DISCRIMINATOR_B_ROPE = Rope.a(new[] { DISCRIMINATOR_B }),
      DISCRIMINATOR_C_ROPE = Rope.a(new[] { DISCRIMINATOR_C });
  }

  class OneOfRW<A, B, C> : ISerializedRW<OneOf<A, B, C>> {
    readonly ISerializedRW<A> aRW;
    readonly ISerializedRW<B> bRW;
    readonly ISerializedRW<C> cRW;

    public OneOfRW(ISerializedRW<A> aRw, ISerializedRW<B> bRw, ISerializedRW<C> cRw) {
      aRW = aRw;
      bRW = bRw;
      cRW = cRw;
    }

    public Option<DeserializeInfo<OneOf<A, B, C>>> deserialize(byte[] serialized, int startIndex) {
      if (serialized.Length == 0 || startIndex > serialized.Length - 1)
        return Option<DeserializeInfo<OneOf<A, B, C>>>.None;
      var discriminator = serialized[startIndex];
      var readFrom = startIndex + 1;
      switch (discriminator) {
        case OneOfRW.DISCRIMINATOR_A:
          return aRW.deserialize(serialized, readFrom).map(info =>
            new DeserializeInfo<OneOf<A, B, C>>(new OneOf<A, B, C>(info.value), info.bytesRead + 1)
          );
        case OneOfRW.DISCRIMINATOR_B:
          return bRW.deserialize(serialized, readFrom).map(info =>
            new DeserializeInfo<OneOf<A, B, C>>(new OneOf<A, B, C>(info.value), info.bytesRead + 1)
          );
        case OneOfRW.DISCRIMINATOR_C:
          return cRW.deserialize(serialized, readFrom).map(info =>
            new DeserializeInfo<OneOf<A, B, C>>(new OneOf<A, B, C>(info.value), info.bytesRead + 1)
          );
        default:
          return Option<DeserializeInfo<OneOf<A, B, C>>>.None;
      }
    }

    public Rope<byte> serialize(OneOf<A, B, C> oneOf) =>
        oneOf.isA ? OneOfRW.DISCRIMINATOR_A_ROPE + aRW.serialize(oneOf.__unsafeGetA)
      : oneOf.isB ? OneOfRW.DISCRIMINATOR_B_ROPE + bRW.serialize(oneOf.__unsafeGetB)
                  : OneOfRW.DISCRIMINATOR_C_ROPE + cRW.serialize(oneOf.__unsafeGetC);
  }
}