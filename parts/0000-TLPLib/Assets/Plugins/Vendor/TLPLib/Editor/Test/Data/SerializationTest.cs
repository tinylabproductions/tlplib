using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  public abstract class SerializationTestBase {
    public static readonly Rope<byte> noise = Rope.create(
      (byte)'a', (byte)'b', (byte)'c', (byte)'d'
    );

    public void checkWithNoise<A>(
      IDeserializer<A> deser, Rope<byte> serialized, A expected
    ) => checkWithNoiseOpt(deser, serialized, expected.some());

    public void checkWithNoiseOpt<A>(
      IDeserializer<A> deser, Rope<byte> serialized, Option<A> expected
    ) => checkWithNoiseOpt(deser, serialized, o => o.shouldEqual(expected));

    public void checkWithNoiseOpt<A>(
      IDeserializer<A> deser, Rope<byte> serialized, Act<Option<A>> check
    ) {
      check(
        deser.deserialize(serialized.toArray(), 0)
        .map(info => valueFromInfo(info, serialized.length))
      );
      check(
        deser.deserialize((noise + serialized + noise).toArray(), noise.length)
        .map(info => valueFromInfo(info, serialized.length))
      );
    }

    static A valueFromInfo<A>(DeserializeInfo<A> info, int serializedLength) {
      info.bytesRead.shouldEqual(serializedLength);
      return info.value;
    }
  }

  public class SerializationTestUShortRW : SerializationTestBase {
    static readonly ISerializedRW<ushort> rw = SerializedRW.uShort;

    [Test]
    public void Test() {
      const ushort value = 8;
      checkWithNoise(rw, rw.serialize(value), value);
    }
  }

  public class SerializationTestByteRW : SerializationTestBase {
    static readonly ISerializedRW<byte> rw = SerializedRW.byte_;

    [Test]
    public void Test() {
      const byte value = 8;
      checkWithNoise(rw, rw.serialize(value), value);
    }
  }

  public class SerializationTestStringRW : SerializationTestBase {
    static readonly ISerializedRW<string> rw = SerializedRW.str;

    [Test]
    public void TestEmpty() {
      var s = "";
      checkWithNoise(rw, rw.serialize(s), s);
    }

    [Test]
    public void TestString() {
      var s = "quickbrownfox";
      checkWithNoise(rw, rw.serialize(s), s);
    }
  }

  public class SerializationTestTplRW : SerializationTestBase {
    static readonly ISerializedRW<Tpl<int, string>> rw = 
      SerializedRW.tpl(SerializedRW.integer, SerializedRW.str);

    [Test]
    public void TestTpl() {
      var t = F.t(1, "foo");
      var serialized = rw.serialize(t);
      checkWithNoise(rw, serialized, t);
    }

    [Test]
    public void TestFailure() =>
      rw.deserialize(noise.toArray(), 0).shouldBeNone();
  }

  public class SerializationTestOptRW : SerializationTestBase {
    static readonly ISerializedRW<Option<int>> rw = SerializedRW.opt(SerializedRW.integer);

    [Test]
    public void TestNone() {
      var serialized = rw.serialize(Option<int>.None);
      checkWithNoise(rw, serialized, Option<int>.None);
    }

    [Test]
    public void TestSome() {
      const int value = int.MaxValue;
      var optVal = F.some(value);
      var serialized = rw.serialize(optVal);
      checkWithNoise(rw, serialized, optVal);
    }

    [Test]
    public void TestFailure() => 
      rw.deserialize(noise.toArray(), 0).shouldBeNone();

  }

  public class SerializationTestEitherRW : SerializationTestBase {
    static readonly ISerializedRW<Either<int, string>> rw = SerializedRW.either(SerializedRW.integer, SerializedRW.str);
    
    [Test]
    public void TestLeft() {
      const int value = int.MaxValue;
      var leftVal = Either<int, string>.Left(value);
      var serialized = rw.serialize(leftVal);
      checkWithNoise(rw, serialized, leftVal);
    }

    [Test]
    public void TestRight() {
      const string value = "test";
      var rightVal = Either<int, string>.Right(value);
      var serialized = rw.serialize(rightVal);
      checkWithNoise(rw, serialized, rightVal);
    }

    [Test]
    public void TestFailure() => rw.deserialize(noise.toArray(), 0).shouldBeNone();
  }

  public class SerializationTestCollection : SerializationTestBase {
    static readonly ISerializer<ICollection<int>> serializer = 
      SerializedRW.collectionSerializer(SerializedRW.integer);

    static readonly IDeserializer<ImmutableArray<int>> deserializer =
      SerializedRW.collectionDeserializer(SerializedRW.integer);

    static readonly IDeserializer<int> failingDeserializer =
      SerializedRW.integer.map(i => (i % 2 == 0).opt(i));

    static readonly ImmutableArray<int> collection = ImmutableArray.Create(1, 2, 3, 4, 5);

    static readonly Rope<byte> serialized = serializer.serialize(collection);

    [Test]
    public void TestNormal() {
      checkWithNoiseOpt(deserializer, serialized, opt => opt.shouldBeSomeEnum(collection));
    }

    [Test]
    public void TestFailing() {
      var deserializer = SerializedRW.collectionDeserializer(failingDeserializer);
      checkWithNoiseOpt(deserializer, serialized, Option<ImmutableArray<int>>.None);
    }
  }
}