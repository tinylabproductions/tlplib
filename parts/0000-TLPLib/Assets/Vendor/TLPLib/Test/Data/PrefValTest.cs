using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using pzdf = pzd.lib.functional;
using pzd.lib.test_framework;
using NUnit.Framework;
using pzd.lib.exts;
using pzd.lib.serialization;
using pzd.lib.typeclasses;
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
      backend.storage[key] = new pzdf.OneOf<string, int, float>("qwerty");

    protected static void ruinBase64(string key) {
      backend.storage[key] = new pzdf.OneOf<string, int, float>(
        backend.storage[key].aValue.get.splice(-1, 1, "!!!!!")
      );
    }
  }

  class PrefValTestCustomString : PrefValTestBase {
    const string key = nameof(PrefValTestCustomString);

    [Test]
    public void Normal() {
      PrefVal<int> create() => 
        storage.custom(key, 3, i => i.ToString(), i => i.parseInt());
      var pv = create();
      pv.value.shouldEqual(3);
      pv.value = 10;
      pv.value.shouldEqual(10);
      var pv2 = create();
      pv2.value.shouldEqual(10);
    }

    [Test]
    public void SerializedIsEmptyString() {
      PrefVal<int> create() => 
        storage.custom(key, 10, _ => "", _ => 1);
      var pv = create();
      pv.value.shouldEqual(10);
      pv.value = 5;
      var pv2 = create();
      pv2.value.shouldEqual(1);
    }

    [Test]
    public void DeserializeFailureReturnDefault() {
      PrefVal<int> create() => storage.custom(
        key, 10, i => i.ToString(), _ => "failed", 
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault, log: log
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
      PrefVal<int> create() => storage.custom(
        key, 10, i => i.ToString(), _ => "failed", 
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException
      );
      var pv = create();
      pv.value.shouldEqual(10);
      pv.value = 5;
      Assert.Throws<SerializationException>(() => create());
    }

    [Test]
    public void SerializedHadPreviousEmptyValueReturnDefault() {
      backend.setString(key, "");
      var pv = storage.custom(
        key, "foo", SerializedRW.str, log: log,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault
      );
      pv.value.shouldEqual("foo", "it should return default value upon deserialization");
      log.warnMsgs.shouldNotBeEmpty("it should log a warning message");
    }

    [Test]
    public void SerializedHadPreviousEmptyValueThrowException() {
      backend.setString(key, "");
      Assert.Throws<SerializationException>(() => storage.custom(
        key, "foo", SerializedRW.str,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException
      ));
    }
  }

  class PrefValTestCustomByteArray : PrefValTestBase {
    const string key = nameof(PrefValTestCustomByteArray);

    [Test]
    public void Normal() {
      PrefVal<string> create() => storage.custom(key, "", SerializedRW.str);
      var pv = create();
      pv.value.shouldEqual("");
      pv.value = "foobar";
      const string key2 = key + "2";
      storage.custom(
        key2, "",
        s => Convert.ToBase64String(SerializedRW.str.serializeToArray(s)),
        _ => pzdf.Either<string, string>.Left("failed")
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
        key, "", SerializedRW.str,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ReturnDefault,
        log: log
      ).value.shouldEqual("");
      log.warnMsgs.shouldNotBeEmpty();
    }

    [Test]
    public void OnDeserializeFailureThrowException() {
      setBadBase64(key);
      Assert.Throws<SerializationException>(() => storage.custom(
        key, "", SerializedRW.str,
        onDeserializeFailure: PrefVal.OnDeserializeFailure.ThrowException,
        log: log
      ));
    }
  }

  class PrefValTestCollection : PrefValTestBase {
    const string key = nameof(PrefValTestCollection);

    class BadRW : ISerializedRW<int> {
      public pzdf.Either<string, DeserializeInfo<int>> deserialize<S>(S stream, IStreamReader<S> reader) =>
        SerializedRW.integer.deserialize(stream, reader).rightValue.filter(i => i.value % 2 != 0).toRight("failed");

      public void serialize<S>(S stream, IStreamWriter<S> writer, int a) =>
        SerializedRW.integer.serialize(stream, writer, a);
    }

    static readonly ImmutableList<int> defaultNonEmpty = ImmutableList.Create(1, 2, 3);

    static PrefVal<ImmutableList<int>> create(
      ImmutableList<int> defaultVal,
      ISerializedRW<int> rw = null,
      PrefVal.OnDeserializeFailure onDeserializeFailure =
        PrefVal.OnDeserializeFailure.ReturnDefault
    ) =>
      storage.collection(
        key, rw ?? SerializedRW.integer,
        CollectionBuilderKnownSizeFactory<int>.immutableList, defaultVal,
        onDeserializeFailure: onDeserializeFailure,
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
    public void ItemDeserializationFailureThrowException() {
      create(ImmutableList.Create(1, 2, 3));
      Assert.Throws<SerializationException>(() =>
        create(
          ImmutableList<int>.Empty,
          new BadRW(),
          PrefVal.OnDeserializeFailure.ThrowException
        )
      );
    }

    [Test]
    public void ItemDeserializationFailureReturnDefault() {
      create(ImmutableList.Create(1, 2, 3));
      var default_ = ImmutableList.Create(1);
      create(
        default_,
        new BadRW(),
        PrefVal.OnDeserializeFailure.ReturnDefault
      ).value.shouldEqual(default_);
    }
  }

  class PrefValHashSetTest : PrefValTestBase {
    [Test]
    public void StringMultipleTimes() {
      Func<PrefVal<ImmutableHashSet<string>>> create = () =>
        storage.hashSet(nameof(PrefValHashSetTest), SerializedRW.str);

      var p1 = create();
      p1.value.shouldEqual(ImmutableHashSet<string>.Empty);
      var l1 = ImmutableHashSet.Create("foo", "bar");
      p1.value = l1;
      p1.value.shouldEqual(l1);
      var l2 = l1.Add("baz");
      p1.value = l2;
      p1.value.shouldEqual(l2);
      var p2 = create();
      p2.value.shouldEqual(l2);
      var l3 = l2.Add("buz");
      p2.value = l3;
      p2.value.shouldEqual(l3);
      var p3 = create();
      p3.value.shouldEqual(l3);
    }
  }
}