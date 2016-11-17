using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public interface ISerializer<in A> {
    Rope<byte> serialize(A a);
  }

  public interface IDeserializer<A> {
    // Returns None if deserialization failed.
    Option<A> deserialize(byte[] serialized, int startIndex);
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
    public static readonly ISerializedRW<Duration> duration = new DurationRW();
    public static readonly ISerializedRW<DateTime> dateTime = new DateTimeRW();

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
      Serialize<A> serialize, Deserialize<A> deserialize
    ) => new Lambda<A>(serialize, deserialize);

    public static ISerializedRW<Tpl<A, B>> tpl<A, B>(
      ISerializedRW<A> aRW, ISerializedRW<B> bRW
    ) => new TplRW<A, B>(aRW, bRW);

    public static ISerializedRW<Option<A>> opt<A>(ISerializedRW<A> rw) => 
      new OptRW<A>(rw);

    public static ISerializer<ICollection<A>> collectionSerializer<A>(ISerializer<A> serializer) =>
      collectionSerializer<A, ICollection<A>>(serializer);

    public static ISerializer<C> collectionSerializer<A, C>(
      ISerializer<A> serializer
    ) where C : ICollection<A> =>
      new ICollectionSerializer<A, C>(serializer);

    public static IDeserializer<ImmutableArray<A>> collectionDeserializer<A>(
      IDeserializer<A> deserializer,
      OnCollectionItemDeserializationFailure onFailure = OnCollectionItemDeserializationFailure.Abort
    ) => new ImmutableArrayDeserializer<A>(deserializer, onFailure);

    class JointRW<A> : ISerializedRW<A> {
      readonly ISerializer<A> serializer;
      readonly IDeserializer<A> deserializer;

      public JointRW(ISerializer<A> serializer, IDeserializer<A> deserializer) {
        this.serializer = serializer;
        this.deserializer = deserializer;
      }

      public Option<A> deserialize(byte[] serialized, int startIndex) =>
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

      public Option<B> deserialize(byte[] serialized, int startIndex) =>
        deserialize(aDeserializer, mapper, serialized, startIndex);

      public static Option<B> deserialize(
        IDeserializer<A> aDeserializer, Fn<A, Option<B>> mapper,
        byte[] serialized, int startIndex
      ) => aDeserializer.deserialize(serialized, startIndex).flatMap(mapper);
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

      public Option<B> deserialize(byte[] serialized, int startIndex) =>
        MappedDeserializer<A, B>.deserialize(aRW, deserializeConversion, serialized, startIndex);

      public Rope<byte> serialize(B b) =>
        MappedSerializer<A, B>.serialize(aRW, serializeConversion, b);
    }

    class Lambda<A> : ISerializedRW<A> {
      readonly Serialize<A> _serialize;
      readonly Deserialize<A> _deserialize;

      public Lambda(Serialize<A> serialize, Deserialize<A> deserialize) {
        _serialize = serialize;
        _deserialize = deserialize;
      }

      public Option<A> deserialize(byte[] serialized, int startIndex) =>
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

      public Option<Tpl<A, B>> deserialize(byte[] serialized, int startIndex) {
        try {
          const int LENGTH_SIZE = 4;
          var length = BitConverter.ToInt32(serialized, startIndex);
          var aOpt = aRW.deserialize(serialized, startIndex + LENGTH_SIZE);
          if (aOpt.isEmpty) return Option<Tpl<A, B>>.None;
          var bOpt = bRW.deserialize(serialized, startIndex + LENGTH_SIZE + length);
          if (bOpt.isEmpty) return Option<Tpl<A, B>>.None;
          return F.some(F.t(aOpt.get, bOpt.get));
        }
        catch (Exception) { return Option<Tpl<A, B>>.None; }
      }

      public Rope<byte> serialize(Tpl<A, B> a) {
        var serializedA = aRW.serialize(a._1);
        var serializedB = bRW.serialize(a._2);
        var length = Rope.a(BitConverter.GetBytes(serializedA.length));

        return length + serializedA + serializedB;
      }
    }

    abstract class BaseRW<A> : ISerializedRW<A> {
      public Option<A> deserialize(byte[] serialized, int startIndex) {
        try { return tryDeserialize(serialized, startIndex).some(); }
        catch (Exception) { return Option<A>.None; }
      }

      protected abstract A tryDeserialize(byte[] serialized, int startIndex);

      public abstract Rope<byte> serialize(A a);
    }

    class stringRW : BaseRW<string> {
      static readonly Encoding encoding = Encoding.UTF8;

      protected override string tryDeserialize(byte[] serialized, int startIndex) =>
        encoding.GetString(serialized, startIndex, serialized.Length - startIndex);

      public override Rope<byte> serialize(string a) => Rope.a(encoding.GetBytes(a));
    }

    class byteRW : ISerializedRW<byte> {
      public Option<byte> deserialize(byte[] serialized, int startIndex) =>
        serialized.get(startIndex);

      public Rope<byte> serialize(byte a) => Rope.a(new [] {a});
    }

    class intRW : BaseRW<int> {
      protected override int tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToInt32(serialized, startIndex);

      public override Rope<byte> serialize(int a) => Rope.a(BitConverter.GetBytes(a));
    }

    class ushortRW : BaseRW<ushort> {
      protected override ushort tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToUInt16(serialized, startIndex);

      public override Rope<byte> serialize(ushort a) => Rope.a(BitConverter.GetBytes(a));
    }

    class uintRW : BaseRW<uint> {
      protected override uint tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToUInt32(serialized, startIndex);

      public override Rope<byte> serialize(uint a) => Rope.a(BitConverter.GetBytes(a));
    }

    class boolRW : BaseRW<bool> {
      protected override bool tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToBoolean(serialized, startIndex);

      public override Rope<byte> serialize(bool a) => Rope.a(BitConverter.GetBytes(a));
    }

    class floatRW : BaseRW<float> {
      protected override float tryDeserialize(byte[] serialized, int startIndex) => 
        BitConverter.ToSingle(serialized, startIndex);

      public override Rope<byte> serialize(float a) => Rope.a(BitConverter.GetBytes(a));
    }

    class longRW : BaseRW<long> {
      protected override long tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToInt64(serialized, startIndex);

      public override Rope<byte> serialize(long a) => Rope.a(BitConverter.GetBytes(a));
    }

    class DurationRW : ISerializedRW<Duration> {
      public Rope<byte> serialize(Duration a) => integer.serialize(a.millis);

      public Option<Duration> deserialize(byte[] serialized, int startIndex) =>
        integer.deserialize(serialized, startIndex).map(millis => new Duration(millis));
    }

    class DateTimeRW : BaseRW<DateTime> {
      protected override DateTime tryDeserialize(byte[] serialized, int startIndex) => 
        DateTime.FromBinary(lng.deserialize(serialized, startIndex).get);

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

      public Option<Option<A>> deserialize(byte[] bytes, int startIndex) {
        if (bytes.Length == 0 || startIndex > bytes.Length - 1)
          return Option<Option<A>>.None;
        var discriminator = bytes[startIndex];
        switch (discriminator) {
          case OptByteArrayRW.DISCRIMINATOR_NONE:
            return F.some(Option<A>.None);
          case OptByteArrayRW.DISCRIMINATOR_SOME:
            return rw.deserialize(bytes, startIndex + 1).map(F.some);
          default:
            return Option<Option<A>>.None;
        }
      }

      public Rope<byte> serialize(Option<A> a) => 
        a.isDefined 
        ? OptByteArrayRW.DISCRIMINATOR_SOME_ROPE + rw.serialize(a.get)
        : OptByteArrayRW.DISCRIMINATOR_NONE_ROPE;
    }

    class ICollectionSerializer<A, C> : ISerializer<C> where C : ICollection<A> {
      readonly ISerializer<A> serializer;

      public ICollectionSerializer(ISerializer<A> serializer) { this.serializer = serializer; }

      public Rope<byte> serialize(C c) {
        var count = c.Count;
        var rope = Rope.a(BitConverter.GetBytes(count));
        foreach (var a in c) {
          var aRope = serializer.serialize(a);
          rope += Rope.a(BitConverter.GetBytes(aRope.length));
          rope += aRope;
        }
        return rope;
      }
    }

    public enum OnCollectionItemDeserializationFailure { Ignore, Abort }

    class ImmutableArrayDeserializer<A> : IDeserializer<ImmutableArray<A>> {
      readonly IDeserializer<A> deserializer;
      readonly OnCollectionItemDeserializationFailure onFailure;

      public ImmutableArrayDeserializer(
        IDeserializer<A> deserializer, OnCollectionItemDeserializationFailure onFailure
      ) {
        this.deserializer = deserializer;
        this.onFailure = onFailure;
      }

      public Option<ImmutableArray<A>> deserialize(byte[] serialized, int startIndex) {
        try {
          const int INT32_LENGTH = 4;
          var count = BitConverter.ToInt32(serialized, startIndex);
          var b = ImmutableArray.CreateBuilder<A>(count);
          var readIdx = startIndex + INT32_LENGTH;
          for (var idx = 0; idx < count; idx++) {
            var length = BitConverter.ToInt32(serialized, readIdx);
            readIdx += INT32_LENGTH;
            var aOpt = deserializer.deserialize(serialized, readIdx);

            if (aOpt.isEmpty) {
              if (onFailure == OnCollectionItemDeserializationFailure.Abort)
                return Option<ImmutableArray<A>>.None;
            }
            else b.Add(aOpt.get);

            readIdx += length;
          }
          // MoveToImmutable throws an exception if capacity != count
          b.Capacity = b.Count;
          return b.MoveToImmutable().some();
        }
        catch (Exception) { return Option<ImmutableArray<A>>.None; }
      }
    }
  }
}