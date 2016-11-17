using System;
using System.Text;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public interface ISerializer<in A> {
    byte[] serialize(A a);
  }

  public interface IDeserializer<A> {
    // Returns None if deserialization failed.
    Option<A> deserialize(byte[] serialized, int startIndex);
  }

  // TODO: document
  public interface ISerializedRW<A> : IDeserializer<A>, ISerializer<A> {}

  public delegate Option<A> Deserialize<A>(byte[] serialized, int startIndex);
  public delegate byte[] Serialize<in A>(A a);

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

    public static ISerializedRW<B> map<A, B>(
      this ISerializedRW<A> aRW,
      Fn<A, Option<B>> deserializeConversion,
      Fn<B, A> serializeConversion
    ) => new Mapped<A, B>(aRW, serializeConversion, deserializeConversion);

    public static ISerializedRW<B> mapTry<A, B>(
      this ISerializedRW<A> aRW,
      Fn<A, B> deserializeConversion,
      Fn<B, A> serializeConversion
    ) => new Mapped<A, B>(aRW, serializeConversion, a => {
      try { return deserializeConversion(a).some(); }
      catch (Exception) { return Option<B>.None; }
    });
    
    public static ISerializedRW<A> lambda<A>(
      Serialize<A> serialize, Deserialize<A> deserialize
    ) => new Lambda<A>(serialize, deserialize);

    public static ISerializedRW<Tpl<A, B>> tpl<A, B>(
      ISerializedRW<A> aRW, ISerializedRW<B> bRW
    ) => new TplRW<A, B>(aRW, bRW);

    class Mapped<A, B> : ISerializedRW<B> {
      readonly ISerializedRW<A> aRW;
      readonly Fn<B, A> serializeConversion;
      readonly Fn<A, Option<B>> deserializeConversion;

      public Mapped(
        ISerializedRW<A> aRw, Fn<B, A> serializeConversion, 
        Fn<A, Option<B>> deserializeConversion
      ) {
        aRW = aRw;
        this.serializeConversion = serializeConversion;
        this.deserializeConversion = deserializeConversion;
      }

      public Option<B> deserialize(byte[] serialized, int startIndex) =>
        aRW.deserialize(serialized, startIndex).flatMap(deserializeConversion);

      public byte[] serialize(B b) =>
        aRW.serialize(serializeConversion(b));
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

      public byte[] serialize(A a) => _serialize(a);
    }

    // TODO: test
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

      public byte[] serialize(Tpl<A, B> a) {
        var serializedA = aRW.serialize(a._1);
        var serializedB = bRW.serialize(a._2);
        var length = BitConverter.GetBytes(serializedA.Length);
        var arr = new byte[length.Length + serializedA.Length + serializedB.Length];
        length.CopyTo(arr, 0);
        serializedA.CopyTo(arr, length.Length);
        serializedB.CopyTo(arr, length.Length + serializedA.Length);
        return arr;
      }
    }

    abstract class BaseRW<A> : ISerializedRW<A> {
      public Option<A> deserialize(byte[] serialized, int startIndex) {
        try { return tryDeserialize(serialized, startIndex).some(); }
        catch (Exception) { return Option<A>.None; }
      }

      protected abstract A tryDeserialize(byte[] serialized, int startIndex);

      public abstract byte[] serialize(A a);
    }

    class stringRW : BaseRW<string> {
      static readonly Encoding encoding = Encoding.UTF8;

      protected override string tryDeserialize(byte[] serialized, int startIndex) =>
        encoding.GetString(serialized, startIndex, serialized.Length - startIndex);

      public override byte[] serialize(string a) => encoding.GetBytes(a);
    }

    class byteRW : ISerializedRW<byte> {
      public Option<byte> deserialize(byte[] serialized, int startIndex) =>
        serialized.get(startIndex);

      public byte[] serialize(byte a) => new[] {a};
    }

    class intRW : BaseRW<int> {
      protected override int tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToInt32(serialized, startIndex);

      public override byte[] serialize(int a) => BitConverter.GetBytes(a);
    }

    class ushortRW : BaseRW<ushort> {
      protected override ushort tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToUInt16(serialized, startIndex);

      public override byte[] serialize(ushort a) => BitConverter.GetBytes(a);
    }

    class uintRW : BaseRW<uint> {
      protected override uint tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToUInt32(serialized, startIndex);

      public override byte[] serialize(uint a) => BitConverter.GetBytes(a);
    }

    class boolRW : BaseRW<bool> {
      protected override bool tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToBoolean(serialized, startIndex);

      public override byte[] serialize(bool a) => BitConverter.GetBytes(a);
    }

    class floatRW : BaseRW<float> {
      protected override float tryDeserialize(byte[] serialized, int startIndex) => 
        BitConverter.ToSingle(serialized, startIndex);

      public override byte[] serialize(float a) => BitConverter.GetBytes(a);
    }

    class longRW : BaseRW<long> {
      protected override long tryDeserialize(byte[] serialized, int startIndex) =>
        BitConverter.ToInt64(serialized, startIndex);

      public override byte[] serialize(long a) => BitConverter.GetBytes(a);
    }

    class DurationRW : ISerializedRW<Duration> {
      public byte[] serialize(Duration a) => integer.serialize(a.millis);

      public Option<Duration> deserialize(byte[] serialized, int startIndex) =>
        integer.deserialize(serialized, startIndex).map(millis => new Duration(millis));
    }

    class DateTimeRW : BaseRW<DateTime> {
      protected override DateTime tryDeserialize(byte[] serialized, int startIndex) => 
        DateTime.FromBinary(lng.deserialize(serialized, startIndex).get);

      public override byte[] serialize(DateTime a) => lng.serialize(a.ToBinary());
    }
    
    // TODO: test
    public class OptByteArrayRW<A> : ISerializedRW<Option<A>> {
      const byte DISCRIMINATOR_NONE = (byte) 'n', DISCRIMINATOR_SOME = (byte) 's';

      readonly ISerializedRW<A> rw;

      public OptByteArrayRW(ISerializedRW<A> rw) { this.rw = rw; }

      public Option<Option<A>> deserialize(byte[] bytes, int startIndex) {
        if (bytes.Length == 0 || startIndex > bytes.Length - 1)
          return Option<Option<A>>.None;
        var discriminator = bytes[startIndex];
        switch (discriminator) {
          case DISCRIMINATOR_NONE:
            return F.some(Option<A>.None);
          case DISCRIMINATOR_SOME:
            return rw.deserialize(bytes, startIndex + 1).map(F.some);
          default:
            return Option<Option<A>>.None;
        }
      }

      public byte[] serialize(Option<A> a) => 
        a.isDefined 
        ? new[] {DISCRIMINATOR_SOME}.concat(rw.serialize(a.get)) 
        : new[] {DISCRIMINATOR_NONE};
    }
  }
}