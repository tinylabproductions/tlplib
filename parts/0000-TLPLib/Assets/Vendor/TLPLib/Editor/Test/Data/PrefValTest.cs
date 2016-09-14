using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace com.tinylabproductions.TLPLib.Data {
  class PrefValTestRamStorage {
    [Test]
    public void ItShouldUpdateTheValueInRam() {
      var key = $"{nameof(ItShouldUpdateTheValueInRam)}-{DateTime.Now.Ticks}";
      var p = PrefVal.player.integer(key, 100);
      p.value.shouldEqual(100);
      p.value = 200;
      p.value.shouldEqual(200);
    }
  }

  class PrefValTestDefaultValueStorage {
    [Test]
    public void ItShouldStoreDefaultValueUponCreation() {
      var key = $"{nameof(ItShouldStoreDefaultValueUponCreation)}-{DateTime.Now.Ticks}";
      var p1 = PrefVal.player.integer(key, Random.Range(0, 100));
      var p2 = PrefVal.player.integer(key, p1.value + 1);
      p2.value.shouldEqual(p1.value);
    }

    [Test]
    public void ItShouldPersistDefaultValueToPrefs() {
      var key = $"{nameof(ItShouldPersistDefaultValueToPrefs)}-{DateTime.Now.Ticks}";
      var p1 = PrefVal.player.integer(key, default(int));
      var p2 = PrefVal.player.integer(key, 10);
      p2.value.shouldEqual(p1.value);
    }
  }

  abstract class PrefValTestBase {
    protected static readonly TestLogger log = new TestLogger();
    protected static readonly IPrefValueTestBackend backend = new IPrefValueTestBackend();
    protected static readonly PrefValStorage storage = new PrefValStorage(backend);

    [SetUp]
    public virtual void SetUp() {
      log.clear();
      backend.storage.Clear();
    }

    protected static void setBadBase64(string key) =>
      backend.storage[key] = new OneOf<string, int, float>("qwerty");

    protected static void ruinBase64(string key) {
      backend.storage[key] = new OneOf<string, int, float>(
        backend.storage[key].aValue.get.splice(-1, 1, "!!!!!")
      );
    }
  }

  class PrefValTestCustomString : PrefValTestBase {
    const string key = nameof(PrefValTestCustomString);

    [Test]
    public void Normal() {
      Fn<PrefVal<int>> create = () => 
        storage.custom(key, 3, i => i.ToString(), i => i.parseInt().rightValue);
      var pv = create();
      pv.value.shouldEqual(3);
      pv.value = 10;
      pv.value.shouldEqual(10);
      var pv2 = create();
      pv2.value.shouldEqual(10);
    }

    [Test]
    public void SerializedIsEmptyString() {
      Fn<PrefVal<int>> create = () =>
        storage.custom(key, 10, _ => "", _ => 1.some());
      var pv = create();
      pv.value.shouldEqual(10);
      pv.value = 5;
      var pv2 = create();
      pv2.value.shouldEqual(1);
    }

    [Test]
    public void DeserializeFailureReturnDefault() {
      Fn<PrefVal<int>> create = () => storage.custom(
        key, 10, i => i.ToString(), _ => Option<int>.None,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault,
        log: log
      );
      var pv = create();
      pv.value.shouldEqual(10);
      pv.value = 5;
      log.warnMsgs.shouldBeEmpty();
      var pv2 = create();
      log.warnMsgs.shouldNotBeEmpty();
      pv2.value.shouldEqual(10);
    }

    [Test]
    public void DeserializeFailureThrowException() {
      Fn<PrefVal<int>> create = () => storage.custom(
        key, 10, i => i.ToString(), _ => Option<int>.None,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException
      );
      var pv = create();
      pv.value.shouldEqual(10);
      pv.value = 5;
      Assert.Throws<SerializationException>(() => create());
    }
  }

  class PrefValTestCustomByteArray : PrefValTestBase {
    const string key = nameof(PrefValTestCustomByteArray);

    [Test]
    public void Normal() {
      Fn<PrefVal<string>> create = () =>
        storage.custom(key, "", PrefVal.stringSerialize, PrefVal.stringDeserialize);
      var pv = create();
      pv.value.shouldEqual("");
      pv.value = "foobar";
      const string key2 = key + "2";
      storage.custom(
        key2, "",
        s => Convert.ToBase64String(PrefVal.stringSerialize(s)),
        _ => Option<string>.None
      ).value = pv.value;
      backend.storage[key].shouldEqual(backend.storage[key2]);
      var pv2 = create();
      pv2.value.shouldEqual(pv.value);
    }

    [Test]
    public void OnDeserializeFailureReturnDefault() {
      setBadBase64(key);
      log.warnMsgs.shouldBeEmpty();
      storage.custom(
        key, "", PrefVal.stringSerialize, PrefVal.stringDeserialize,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault,
        log: log
      ).value.shouldEqual("");
      log.warnMsgs.shouldNotBeEmpty();
    }

    [Test]
    public void OnDeserializeFailureThrowException() {
      setBadBase64(key);
      Assert.Throws<SerializationException>(() => storage.custom(
        key, "", PrefVal.stringSerialize, PrefVal.stringDeserialize,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException,
        log: log
      ));
    }
  }

  class PrefValTestBase64 : PrefValTestBase {
    const string key = nameof(PrefValTestBase64);

    static IEnumerable<byte[]> serialize(ImmutableList<int> l) =>
      l.Select(BitConverter.GetBytes);

    static Option<ImmutableList<int>> deserialize(IEnumerable<byte[]> e) =>
      ImmutableList.CreateRange(
        e.SelectMany(_ => _.toInt().value.asEnum())
      ).some();

    [Test]
    public void Normal() {
      Fn<PrefVal<ImmutableList<int>>> create = () =>
        storage.base64(key, ImmutableList<int>.Empty, serialize, deserialize);
      var data = ImmutableList.Create(1, 2, 3, 4);
      var pv = create();
      pv.value.shouldBeEmpty();
      pv.value = data;
      var pv2 = create();
      pv2.value.shouldEqual(data);
    }

    [Test]
    public void DeserializeFailureReturnDefault() {
      setBadBase64(key);
      var defaultVal = ImmutableList.Create(1, 2, 3, 4);
      log.warnMsgs.shouldBeEmpty();
      var pv = storage.base64(
        key, defaultVal, serialize, deserialize,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault,
        log: log
      );
      log.warnMsgs.shouldNotBeEmpty();
      pv.value.shouldEqual(defaultVal);
    }

    [Test]
    public void DeserializeFailureThrowException() {
      setBadBase64(key);
      Assert.Throws<SerializationException>(() => storage.base64(
        key, ImmutableList<int>.Empty, serialize, deserialize,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException
      ));
    }

    [Test]
    public void DeserializePartFailedReturnDefault() {
      var defaultVal = ImmutableList.Create(1, 2, 3, 4);
      Fn<PrefVal<ImmutableList<int>>> create = () => storage.base64(
        key, defaultVal, serialize, deserialize,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault,
        log: log
      );
      var pv = create();
      pv.value = ImmutableList.Create(1, 2, 3, 4, 5);
      log.warnMsgs.shouldBeEmpty();
      ruinBase64(key);
      var pv2 = create();
      log.warnMsgs.shouldNotBeEmpty();
      pv2.value.shouldEqual(defaultVal);
    }

    [Test]
    public void DeserializePartFailedThrowException() {
      Fn<PrefVal<ImmutableList<int>>> create = () => storage.base64(
        key, ImmutableList.Create(1, 2, 3, 4), serialize, deserialize,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException
      );
      create();
      ruinBase64(key);
      Assert.Throws<SerializationException>(() => create());
    }
  }

  class PrefValTestCollection : PrefValTestBase {
    const string key = nameof(PrefValTestCollection);

    static byte[] serialize(int i) => BitConverter.GetBytes(i);
    static Option<int> deserialize(byte[] data) => data.toInt().value;
    static Option<int> badDeserialize(byte[] data) => deserialize(data).filter(i => i % 2 != 0);
    static ImmutableList<int>.Builder createBuilder() => ImmutableList.CreateBuilder<int>();
    static void add(ImmutableList<int>.Builder builder, int value) => builder.Add(value);
    static ImmutableList<int> toList(ImmutableList<int>.Builder builder) => builder.ToImmutable();
    static readonly ImmutableList<int> defaultNonEmpty = ImmutableList.Create(1, 2, 3);

    PrefVal<ImmutableList<int>> create(
      ImmutableList<int> defaultVal,
      Fn<byte[], Option<int>> deserializeFn = null,
      PrefVal.OnDeserializeCollectionItemFailure onItemFailure = 
        PrefVal.OnDeserializeCollectionItemFailure.ThrowException
    ) =>
      storage.collection(
        key, serialize, deserializeFn ?? deserialize, 
        createBuilder, add, toList, defaultVal,
        onDeserializeCollectionItemFailure: onItemFailure,
        log: log
      );

    [Test]
    public void WithDefaultValue() {
      create(defaultNonEmpty).value.shouldEqual(defaultNonEmpty);
      create(ImmutableList<int>.Empty).value.shouldEqual(defaultNonEmpty);
    }

    [Test]
    public void WithEmptyCollection() {
      create(defaultNonEmpty).value = ImmutableList<int>.Empty;
      create(defaultNonEmpty).value.shouldEqual(ImmutableList<int>.Empty);
    }

    [Test]
    public void WithDefaultEmpty() {
      create(ImmutableList<int>.Empty).value.shouldEqual(ImmutableList<int>.Empty);
      create(defaultNonEmpty).value.shouldEqual(ImmutableList<int>.Empty);
    }

    [Test]
    public void Normal() {
      var p = create(ImmutableList<int>.Empty);
      p.value.shouldEqual(ImmutableList<int>.Empty);
      var v = ImmutableList.Create(4, 5, 6, 7);
      p.value = v;
      p.value.shouldEqual(v);
      var p1 = create(ImmutableList<int>.Empty);
      p1.value.shouldEqual(v);
    }

    [Test]
    public void ItemDeserializationFailureIgnore() {
      create(ImmutableList.Create(1, 2, 3));
      log.warnMsgs.shouldBeEmpty();
      var p1 = create(
        ImmutableList<int>.Empty,
        badDeserialize,
        PrefVal.OnDeserializeCollectionItemFailure.Ignore
      );
      p1.value.shouldEqual(ImmutableList.Create(1, 3));
      log.warnMsgs.shouldNotBeEmpty();
    }

    [Test]
    public void ItemDeserializationFailureThrowException() {
      create(ImmutableList.Create(1, 2, 3));
      Assert.Throws<SerializationException>(() =>
        create(
          ImmutableList<int>.Empty,
          badDeserialize,
          PrefVal.OnDeserializeCollectionItemFailure.ThrowException
        )
      );
    }
  }
}