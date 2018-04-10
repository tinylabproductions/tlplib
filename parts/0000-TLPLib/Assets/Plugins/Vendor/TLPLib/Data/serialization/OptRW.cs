using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data.serialization {
  static class OptRW {
    public const byte
      DISCRIMINATOR_NONE = (byte) 'n',
      DISCRIMINATOR_SOME = (byte) 's';

    public static readonly Rope<byte>
      DISCRIMINATOR_NONE_ROPE = Rope.a(new[] { DISCRIMINATOR_NONE }),
      DISCRIMINATOR_SOME_ROPE = Rope.a(new[] { DISCRIMINATOR_SOME });
  }

  class OptRW<A> : ISerializedRW<Option<A>> {
    readonly ISerializedRW<A> rw;

    public OptRW(ISerializedRW<A> rw) { this.rw = rw; }

    public Option<DeserializeInfo<Option<A>>> deserialize(byte[] bytes, int startIndex) {
      if (bytes.Length == 0 || startIndex > bytes.Length - 1)
        return Option<DeserializeInfo<Option<A>>>.None;
      var discriminator = bytes[startIndex];
      switch (discriminator) {
        case OptRW.DISCRIMINATOR_NONE:
          return F.some(new DeserializeInfo<Option<A>>(Option<A>.None, 1));
        case OptRW.DISCRIMINATOR_SOME:
          return rw.deserialize(bytes, startIndex + 1).map(info =>
            new DeserializeInfo<Option<A>>(F.some(info.value), info.bytesRead + 1)
          );
        default:
          return Option<DeserializeInfo<Option<A>>>.None;
      }
    }

    public Rope<byte> serialize(Option<A> a) =>
      a.isSome
        ? OptRW.DISCRIMINATOR_SOME_ROPE + rw.serialize(a.get)
        : OptRW.DISCRIMINATOR_NONE_ROPE;
  }
}