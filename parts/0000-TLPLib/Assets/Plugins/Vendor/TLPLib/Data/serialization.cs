using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Data {
  public interface ISerializer<in A> {
    Rope<byte> serialize(A a);
  }

  public struct DeserializeInfo<A> {
    public readonly A value;
    public readonly int bytesRead;

    public DeserializeInfo(A value, int bytesRead) {
      this.value = value;
      this.bytesRead = bytesRead;
    }

    public Option<DeserializeInfo<B>> flatMapTry<B>(Fn<A, B> mapper) {
      try { return new DeserializeInfo<B>(mapper(value), bytesRead).some(); }
      catch (Exception) { return Option<DeserializeInfo<B>>.None; }
    }
  }

  public static class DeserializeInfoExts {
    public static Option<DeserializeInfo<B>> map<A, B>(
      this Option<DeserializeInfo<A>> aOpt, Fn<A, B> mapper
    ) {
      if (aOpt.isNone) return Option<DeserializeInfo<B>>.None;
      var aInfo = aOpt.get;
      return F.some(new DeserializeInfo<B>(mapper(aInfo.value), aInfo.bytesRead));
    }
  }

  public interface IDeserializer<A> {
    // Returns None if deserialization failed.
    Option<DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex);
  }

  // TODO: document
  public interface ISerializedRW<A> : IDeserializer<A>, ISerializer<A> {}

  public delegate Option<A> Deserialize<A>(byte[] serialized, int startIndex);
  public delegate Rope<byte> Serialize<in A>(A a);

  public static class SerializedRW {
    public static readonly ISerializedRW<string> str = new stringRW();
    public static readonly ISerializedRW<int> integer = new intRW();
    public static readonly ISerializedRW<byte> byte_ = new byteRW();
    public static readonly ISerializedRW<uint> uInteger = new uintRW();
    public static readonly ISerializedRW<ushort> uShort = new ushortRW();
    public static readonly ISerializedRW<bool> boolean = new boolRW();
    public static readonly ISerializedRW<float> flt = new floatRW();
    public static readonly ISerializedRW<long> lng = new longRW();
    public static readonly ISerializedRW<DateTime> dateTime = new DateTimeRW();
    public static readonly ISerializedRW<Uri> uri = lambda(
      uri => str.serialize(uri.ToString()),
      (bytes, startIndex) =>
        str.deserialize(bytes, startIndex)
        .flatMap(di => di.flatMapTry(s => new Uri(s)))
    );

#if UNITY_EDITOR
    public static ISerializedRW<A> unityObjectSerializedRW<A>() where A : UnityEngine.Object => PathStr.serializedRW.map(
       path => AssetDatabase.LoadAssetAtPath<A>(path).opt(),
       module => module.path()
    );
#endif

    public static ISerializedRW<Unit> unit => UnitRW.instance;

    /// <summary>Serialized RW for a type that has no parameters (like <see cref="Unit"/>)</summary>
    public static ISerializedRW<A> unitType<A>() where A : new() =>
      unit.map(_ => F.some(new A()), _ => F.unit);

    public static ISerializedRW<A> a<A>(
      ISerializer<A> serializer, IDeserializer<A> deserializer
    ) => new JointRW<A>(serializer, deserializer);

    public static ISerializer<B> map<A, B>(
      this ISerializer<A> a, Fn<B, A> mapper
    ) => new MappedSerializer<A,B>(a, mapper);

    public static IDeserializer<B> map<A, B>(
      this IDeserializer<A> a, Fn<A, Option<B>> mapper
    ) => new MappedDeserializer<A, B>(a, mapper);

    public static ISerializedRW<B> map<A, B>(
      this ISerializedRW<A> aRW,
      Fn<A, Option<B>> deserializeConversion,
      Fn<B, A> serializeConversion
    ) => new MappedRW<A, B>(aRW, serializeConversion, deserializeConversion);

    public static ISerializedRW<B> mapTry<A, B>(
      this ISerializedRW<A> aRW,
      Fn<A, B> deserializeConversion,
      Fn<B, A> serializeConversion
    ) => new MappedRW<A, B>(aRW, serializeConversion, a => {
      try { return deserializeConversion(a).some(); }
      catch (Exception) { return Option<B>.None; }
    });

    public static ISerializedRW<A> lambda<A>(
      Serialize<A> serialize, Deserialize<DeserializeInfo<A>> deserialize
    ) => new Lambda<A>(serialize, deserialize);

    public static ISerializedRW<OneOf<A, B, C>> oneOf<A, B, C>(
      ISerializedRW<A> aRW, ISerializedRW<B> bRW, ISerializedRW<C> cRW
    ) => new OneOfRW<A, B, C>(aRW, bRW, cRW);

    public static ISerializedRW<Tpl<A, B>> tpl<A, B>(
      ISerializedRW<A> aRW, ISerializedRW<B> bRW
    ) => new TplRW<A, B>(aRW, bRW);

    public static ISerializedRW<Tpl<A, B>> and<A, B>(
      this ISerializedRW<A> aRW, ISerializedRW<B> bRW
    ) => tpl(aRW, bRW);

    public static ISerializedRW<Option<A>> opt<A>(ISerializedRW<A> rw) =>
      new OptRW<A>(rw);

    public static ISerializedRW<Either<A, B>> either<A, B>(ISerializedRW<A> aRW, ISerializedRW<B> bRW) =>
      new EitherRW<A, B>(aRW, bRW);

    public static ISerializedRW<ImmutableArray<A>> immutableArray<A>(
      ISerializedRW<A> rw
    ) => a(collectionSerializer<A, ImmutableArray<A>>(rw), collectionDeserializer(rw));

    public static ISerializer<ICollection<A>> collectionSerializer<A>(ISerializer<A> serializer) =>
      collectionSerializer<A, ICollection<A>>(serializer);

    public static ISerializer<C> collectionSerializer<A, C>(
      ISerializer<A> serializer
    ) where C : ICollection<A> =>
      new ICollectionSerializer<A, C>(serializer);

    public static IDeserializer<ImmutableArray<A>> collectionDeserializer<A>(
      IDeserializer<A> deserializer
    ) => new ImmutableArrayDeserializer<A>(deserializer);

    class UnitRW : ISerializedRW<Unit> {
      public static readonly UnitRW instance = new UnitRW();
      UnitRW() {}

      static readonly Rope<byte> UNIT_ROPE = Rope.a(new byte[0]);
      static readonly Option<DeserializeInfo<Unit>> DESERIALIZE_INFO =
        F.some(new DeserializeInfo<Unit>(F.unit, 0));

      public Rope<byte> serialize(Unit a) => UNIT_ROPE;
      public Option<DeserializeInfo<Unit>> deserialize(byte[] serialized, int startIndex) =>
        DESERIALIZE_INFO;
    }

    class JointRW<A> : ISerializedRW<A> {
      readonly ISerializer<A> serializer;
      readonly IDeserializer<A> deserializer;

      public JointRW(ISerializer<A> serializer, IDeserializer<A> deserializer) {
        this.serializer = serializer;
        this.deserializer = deserializer;
      }

      public Option<DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex) =>
        deserializer.deserialize(serialized, startIndex);

      public Rope<byte> serialize(A a) =>
        serializer.serialize(a);
    }

    class MappedSerializer<A, B> : ISerializer<B> {
      readonly ISerializer<A> aSerializer;
      readonly Fn<B, A> mapper;

      public MappedSerializer(ISerializer<A> aSerializer, Fn<B, A> mapper) {
        this.aSerializer = aSerializer;
        this.mapper = mapper;
      }

      public static Rope<byte> serialize(ISerializer<A> aSer, Fn<B, A> mapper, B b) =>
        aSer.serialize(mapper(b));

      public Rope<byte> serialize(B b) => serialize(aSerializer, mapper, b);
    }

    class MappedDeserializer<A, B> : IDeserializer<B> {
      readonly IDeserializer<A> aDeserializer;
      readonly Fn<A, Option<B>> mapper;

      public MappedDeserializer(IDeserializer<A> aDeserializer, Fn<A, Option<B>> mapper) {
        this.aDeserializer = aDeserializer;
        this.mapper = mapper;
      }

      public Option<DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) =>
        deserialize(aDeserializer, mapper, serialized, startIndex);

      public static Option<DeserializeInfo<B>> deserialize(
        IDeserializer<A> aDeserializer, Fn<A, Option<B>> mapper,
        byte[] serialized, int startIndex
      ) {
        var aInfoOpt = aDeserializer.deserialize(serialized, startIndex);
        if (aInfoOpt.isNone) return Option<DeserializeInfo<B>>.None;
        var aInfo = aInfoOpt.get;
        var bOpt = mapper(aInfo.value);
        if (bOpt.isNone) return Option<DeserializeInfo<B>>.None;
        var bInfo = new DeserializeInfo<B>(bOpt.get, aInfo.bytesRead);
        return F.some(bInfo);
      }
    }

    class MappedRW<A, B> : ISerializedRW<B> {
      readonly ISerializedRW<A> aRW;
      readonly Fn<B, A> serializeConversion;
      readonly Fn<A, Option<B>> deserializeConversion;

      public MappedRW(
        ISerializedRW<A> aRw, Fn<B, A> serializeConversion,
        Fn<A, Option<B>> deserializeConversion
      ) {
        aRW = aRw;
        this.serializeConversion = serializeConversion;
        this.deserializeConversion = deserializeConversion;
      }

      public Option<DeserializeInfo<B>> deserialize(byte[] serialized, int startIndex) =>
        MappedDeserializer<A, B>.deserialize(aRW, deserializeConversion, serialized, startIndex);

      public Rope<byte> serialize(B b) =>
        MappedSerializer<A, B>.serialize(aRW, serializeConversion, b);
    }

    class Lambda<A> : ISerializedRW<A> {
      readonly Serialize<A> _serialize;
      readonly Deserialize<DeserializeInfo<A>> _deserialize;

      public Lambda(Serialize<A> serialize, Deserialize<DeserializeInfo<A>> deserialize) {
        _serialize = serialize;
        _deserialize = deserialize;
      }

      public Option<DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex) =>
        _deserialize(serialized, startIndex);

      public Rope<byte> serialize(A a) => _serialize(a);
    }

    class TplRW<A, B> : ISerializedRW<Tpl<A, B>> {
      readonly ISerializedRW<A> aRW;
      readonly ISerializedRW<B> bRW;

      public TplRW(ISerializedRW<A> aRw, ISerializedRW<B> bRw) {
        aRW = aRw;
        bRW = bRw;
      }

      public Option<DeserializeInfo<Tpl<A, B>>> deserialize(byte[] serialized, int startIndex) {
        try {
          var aOpt = aRW.deserialize(serialized, startIndex);
          if (aOpt.isNone) return Option<DeserializeInfo<Tpl<A, B>>>.None;
          var aInfo = aOpt.get;
          var bOpt = bRW.deserialize(serialized, startIndex + aInfo.bytesRead);
          if (bOpt.isNone) return Option<DeserializeInfo<Tpl<A, B>>>.None;
          var bInfo = bOpt.get;
          var info = new DeserializeInfo<Tpl<A, B>>(
            F.t(aInfo.value, bInfo.value),
            aInfo.bytesRead + bInfo.bytesRead
          );
          return F.some(info);
        }
        catch (Exception) { return Option<DeserializeInfo<Tpl<A, B>>>.None; }
      }

      public Rope<byte> serialize(Tpl<A, B> a) =>
        aRW.serialize(a._1) + bRW.serialize(a._2);
    }

    abstract class BaseRW<A> : ISerializedRW<A> {
      public Option<DeserializeInfo<A>> deserialize(byte[] serialized, int startIndex) {
        try { return tryDeserialize(serialized, startIndex).some(); }
        catch (Exception) { return Option<DeserializeInfo<A>>.None; }
      }

      protected abstract DeserializeInfo<A> tryDeserialize(byte[] serialized, int startIndex);

      public abstract Rope<byte> serialize(A a);
    }

    class stringRW : BaseRW<string> {
      static readonly Encoding encoding = Encoding.UTF8;

      // TODO: test
      protected override DeserializeInfo<string> tryDeserialize(byte[] serialized, int startIndex) {
        var length = BitConverter.ToInt32(serialized, startIndex);
        var str = encoding.GetString(serialized, startIndex + intRW.LENGTH, length);
        return new DeserializeInfo<string>(str, intRW.LENGTH + length);
      }

      public override Rope<byte> serialize(string a) {
        var serialized = encoding.GetBytes(a);
        var length = BitConverter.GetBytes(serialized.Length);
        return Rope.a(length, serialized);
      }
    }

    class byteRW : ISerializedRW<byte> {
      public Option<DeserializeInfo<byte>> deserialize(byte[] serialized, int startIndex) =>
        serialized.get(startIndex).map(b => new DeserializeInfo<byte>(b, 1));

      public Rope<byte> serialize(byte a) => Rope.a(new [] {a});
    }

    class intRW : BaseRW<int> {
      public const int LENGTH = 4;

      protected override DeserializeInfo<int> tryDeserialize(byte[] serialized, int startIndex) =>
        new DeserializeInfo<int>(BitConverter.ToInt32(serialized, startIndex), LENGTH);

      public override Rope<byte> serialize(int a) => Rope.a(BitConverter.GetBytes(a));
    }

    class ushortRW : BaseRW<ushort> {
      public const int LENGTH = 2;

      protected override DeserializeInfo<ushort> tryDeserialize(byte[] serialized, int startIndex) =>
        new DeserializeInfo<ushort>(BitConverter.ToUInt16(serialized, startIndex), LENGTH);

      public override Rope<byte> serialize(ushort a) => Rope.a(BitConverter.GetBytes(a));
    }

    class uintRW : BaseRW<uint> {
      public const int LENGTH = 4;

      protected override DeserializeInfo<uint> tryDeserialize(byte[] serialized, int startIndex) =>
        new DeserializeInfo<uint>(BitConverter.ToUInt32(serialized, startIndex), LENGTH);

      public override Rope<byte> serialize(uint a) => Rope.a(BitConverter.GetBytes(a));
    }

    class boolRW : BaseRW<bool> {
      public const int LENGTH = 1;

      protected override DeserializeInfo<bool> tryDeserialize(byte[] serialized, int startIndex) =>
        new DeserializeInfo<bool>(BitConverter.ToBoolean(serialized, startIndex), LENGTH);

      public override Rope<byte> serialize(bool a) => Rope.a(BitConverter.GetBytes(a));
    }

    class floatRW : BaseRW<float> {
      public const int LENGTH = 4;

      protected override DeserializeInfo<float> tryDeserialize(byte[] serialized, int startIndex) =>
        new DeserializeInfo<float>(BitConverter.ToSingle(serialized, startIndex), LENGTH);

      public override Rope<byte> serialize(float a) => Rope.a(BitConverter.GetBytes(a));
    }

    class longRW : BaseRW<long> {
      public const int LENGTH = 8;

      protected override DeserializeInfo<long> tryDeserialize(byte[] serialized, int startIndex) =>
        new DeserializeInfo<long>(BitConverter.ToInt64(serialized, startIndex), LENGTH);

      public override Rope<byte> serialize(long a) => Rope.a(BitConverter.GetBytes(a));
    }

    class DateTimeRW : BaseRW<DateTime> {
      protected override DeserializeInfo<DateTime> tryDeserialize(byte[] serialized, int startIndex) =>
        lng.deserialize(serialized, startIndex).map(DateTime.FromBinary).get;

      public override Rope<byte> serialize(DateTime a) => lng.serialize(a.ToBinary());
    }

    static class OptByteArrayRW {
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
          case OptByteArrayRW.DISCRIMINATOR_NONE:
            return F.some(new DeserializeInfo<Option<A>>(Option<A>.None, 1));
          case OptByteArrayRW.DISCRIMINATOR_SOME:
            return rw.deserialize(bytes, startIndex + 1).map(info =>
              new DeserializeInfo<Option<A>>(F.some(info.value), info.bytesRead + 1)
            );
          default:
            return Option<DeserializeInfo<Option<A>>>.None;
        }
      }

      public Rope<byte> serialize(Option<A> a) =>
        a.isSome
        ? OptByteArrayRW.DISCRIMINATOR_SOME_ROPE + rw.serialize(a.get)
        : OptByteArrayRW.DISCRIMINATOR_NONE_ROPE;
    }

    static class EitherRW {
      public const byte
        DISCRIMINATOR_LEFT = (byte) 'l',
        DISCRIMINATOR_RIGHT = (byte) 'r';

      public static readonly Rope<byte>
        DISCRIMINATOR_LEFT_ROPE = Rope.a(new[] { DISCRIMINATOR_LEFT }),
        DISCRIMINATOR_RIGHT_ROPE = Rope.a(new[] { DISCRIMINATOR_RIGHT });
    }

    class EitherRW<A, B> : ISerializedRW<Either<A, B>> {
      readonly ISerializedRW<A> aRW;
      readonly ISerializedRW<B> bRW;

      public EitherRW(ISerializedRW<A> aRw, ISerializedRW<B> bRw) {
        aRW = aRw;
        bRW = bRw;
      }

      public Option<DeserializeInfo<Either<A, B>>> deserialize(byte[] serialized, int startIndex) {
        if (serialized.Length == 0 || startIndex > serialized.Length - 1)
          return Option<DeserializeInfo<Either<A, B>>>.None;
        var discriminator = serialized[startIndex];
        switch (discriminator) {
          case EitherRW.DISCRIMINATOR_LEFT:
            return aRW.deserialize(serialized, startIndex + 1).map(info =>
              new DeserializeInfo<Either<A, B>>(Either<A, B>.Left(info.value), info.bytesRead + 1)
            );
          case EitherRW.DISCRIMINATOR_RIGHT:
            return bRW.deserialize(serialized, startIndex + 1).map(info =>
              new DeserializeInfo<Either<A, B>>(Either<A,B>.Right(info.value), info.bytesRead + 1)
            );
          default:
            return Option<DeserializeInfo<Either<A, B>>>.None;
        }
      }

      public Rope<byte> serialize(Either<A, B> either) =>
        either.isLeft
        ? EitherRW.DISCRIMINATOR_LEFT_ROPE + aRW.serialize(either.__unsafeGetLeft)
        : EitherRW.DISCRIMINATOR_RIGHT_ROPE + bRW.serialize(either.__unsafeGetRight);
    }

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

    class ICollectionSerializer<A, C> : ISerializer<C> where C : ICollection<A> {
      readonly ISerializer<A> serializer;

      public ICollectionSerializer(ISerializer<A> serializer) { this.serializer = serializer; }

      public Rope<byte> serialize(C c) {
        var count = c.Count;
        var rope = Rope.a(BitConverter.GetBytes(count));
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var a in c) {
          var aRope = serializer.serialize(a);
          rope += aRope;
        }
        return rope;
      }
    }

    class ImmutableArrayDeserializer<A> : IDeserializer<ImmutableArray<A>> {
      readonly IDeserializer<A> deserializer;

      public ImmutableArrayDeserializer(
        IDeserializer<A> deserializer
      ) {
        this.deserializer = deserializer;
      }

      public Option<DeserializeInfo<ImmutableArray<A>>> deserialize(byte[] serialized, int startIndex) {
        try {
          var count = BitConverter.ToInt32(serialized, startIndex);
          var bytesRead = intRW.LENGTH;

          var builder = ImmutableArray.CreateBuilder<A>(count);
          var readIdx = startIndex + bytesRead;
          for (var idx = 0; idx < count; idx++) {
            var aOpt = deserializer.deserialize(serialized, readIdx);

            if (aOpt.isNone) {
              return Option<DeserializeInfo<ImmutableArray<A>>>.None;
            }
            var aInfo = aOpt.get;
            bytesRead += aInfo.bytesRead;
            readIdx += aInfo.bytesRead;
            builder.Add(aInfo.value);
          }
          return F.some(new DeserializeInfo<ImmutableArray<A>>(builder.MoveToImmutableSafe(), bytesRead));
        }
        catch (Exception) { return Option<DeserializeInfo<ImmutableArray<A>>>.None; }
      }
    }
  }
}
