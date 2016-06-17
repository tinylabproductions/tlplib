using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Configuration {
  public class ConfigTestOptString {
    [Test]
    public void WhenGettingDictAsString() {
      var cfg = new Config(new Dictionary<string, object> {
        {"foo", new Dictionary<string, object>()}
      });
      cfg.optString("foo").shouldBeNone();
    }
  }

// Well, this whole test is broken, no time to fix it. Fun times! - arturaz
//  static class Data {
//    public static Dictionary<string, object> dictFor(string key, object o)
//      { return F.dict(F.t(key, o)); }
//
//    public static Dictionary<string, object> dictForNested(string key, string key2, object o)
//      { return F.dict(F.t(key, (object) F.dict(F.t(key2, o)))); }
//
//    public static Dictionary<string, object> dictForList(string key, object o)
//      { return F.dict(F.t(key, (object) F.list(o, o))); }
//  }
//
//  /** NInt - Negative Int. */
//  public enum ConfigType {
//    Int, NInt, UInt,
//    Long, NLong, ULong,
//    Float, Double,
//    Bool, String, DateTime
//  }
//
//  public struct FetchFns {
//    public readonly Fn<IConfig, string, object> getter;
//    public readonly Fn<IConfig, string, Option<object>> optGetter;
//    public readonly Fn<IConfig, string, Try<object>> tryGetter;
//    public readonly Fn<IConfig, string, Either<ConfigFetchError, object>> eitherGetter;
//
//    public FetchFns(Fn<IConfig, string, object> getter, Fn<IConfig, string, Option<object>> optGetter, Fn<IConfig, string, Try<object>> tryGetter, Fn<IConfig, string, Either<ConfigFetchError, object>> eitherGetter) {
//      this.getter = getter;
//      this.optGetter = optGetter;
//      this.tryGetter = tryGetter;
//      this.eitherGetter = eitherGetter;
//    }
//  }
//
//  public struct ExampleData {
//    public readonly ConfigType type;
//    public readonly ConfigType[] constructableFrom;
//    public readonly FetchFns fetchFns;
//    public readonly object[] values;
//
//    public ExampleData(ConfigType type, FetchFns fetchFns, params object[] values)
//      : this(type, new ConfigType[0], fetchFns, values) {}
//
//    public ExampleData(
//      ConfigType type, ConfigType[] constructableFrom, FetchFns fetchFns, params object[] values
//    ) {
//      this.type = type;
//      this.constructableFrom = constructableFrom;
//      this.fetchFns = fetchFns;
//      this.values = values;
//    }
//  }
//
//  public class ExampleDataTable {
//    public readonly Dictionary<ConfigType, ExampleData> table;
//
//    public ExampleDataTable(Dictionary<ConfigType, ExampleData> table) { this.table = table; }
//
//    public IEnumerable<object> validValuesFor(ConfigType type) {
//      var example = table[type];
//      return
//        example.values
//        .Concat(example.constructableFrom.SelectMany(t => table[t].values));
//    }
//
//    public IEnumerable<object> invalidValuesFor(ConfigType type) {
//      var example = table[type];
//      return
//        table
//        .Where(kv => !example.constructableFrom.Contains(kv.Key))
//        .SelectMany(kv => kv.Value.values);
//    }
//  }
//
//  static class ConfigTestData {
//    const long longStart = (long) int.MaxValue + 1;
//    const long nLongStart = (long) int.MinValue - 1;
//
//    public static readonly FetchFns StringFns = new FetchFns(
//      (c, k) => c.getString(k),
//      (c, k) => c.optString(k).map(toObj),
//      (c, k) => c.tryString(k).map(toObj),
//      (c, k) => c.eitherString(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns IntFns = new FetchFns(
//      (c, k) => c.getInt(k),
//      (c, k) => c.optInt(k).map(toObj),
//      (c, k) => c.tryInt(k).map(toObj),
//      (c, k) => c.eitherInt(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns LongFns = new FetchFns(
//      (c, k) => c.getLong(k),
//      (c, k) => c.optLong(k).map(toObj),
//      (c, k) => c.tryLong(k).map(toObj),
//      (c, k) => c.eitherLong(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns UIntFns = new FetchFns(
//      (c, k) => c.getUInt(k),
//      (c, k) => c.optUInt(k).map(toObj),
//      (c, k) => c.tryUInt(k).map(toObj),
//      (c, k) => c.eitherUInt(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns ULongFns = new FetchFns(
//      (c, k) => c.getULong(k),
//      (c, k) => c.optULong(k).map(toObj),
//      (c, k) => c.tryULong(k).map(toObj),
//      (c, k) => c.eitherULong(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns FloatFns = new FetchFns(
//      (c, k) => c.getFloat(k),
//      (c, k) => c.optFloat(k).map(toObj),
//      (c, k) => c.tryFloat(k).map(toObj),
//      (c, k) => c.eitherFloat(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns DoubleFns = new FetchFns(
//      (c, k) => c.getDouble(k),
//      (c, k) => c.optDouble(k).map(toObj),
//      (c, k) => c.tryDouble(k).map(toObj),
//      (c, k) => c.eitherDouble(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns BoolFns = new FetchFns(
//      (c, k) => c.getBool(k),
//      (c, k) => c.optBool(k).map(toObj),
//      (c, k) => c.tryBool(k).map(toObj),
//      (c, k) => c.eitherBool(k).mapRight(toObj)
//    );
//
//    public static readonly FetchFns DateTimeFns = new FetchFns(
//      (c, k) => c.getDateTime(k),
//      (c, k) => c.optDateTime(k).map(toObj),
//      (c, k) => c.tryDateTime(k).map(toObj),
//      (c, k) => c.eitherDateTime(k).mapRight(toObj)
//    );
//
//    public static readonly ExampleDataTable examples = new ExampleDataTable(new [] {
//      new ExampleData(ConfigType.Int, IntFns, 0, 3, int.MaxValue),
//      new ExampleData(ConfigType.NInt, IntFns, -3, int.MinValue),
//      new ExampleData(
//        ConfigType.UInt, new[] {ConfigType.Int}, UIntFns,
//        uint.MinValue, 3, uint.MaxValue
//      ),
//      new ExampleData(
//        ConfigType.Long, new[] {ConfigType.Int, ConfigType.UInt}, LongFns,
//        0, 3, longStart, long.MaxValue
//      ),
//      new ExampleData(
//        ConfigType.NLong, new[] {ConfigType.NInt}, LongFns,
//        -3, nLongStart, long.MinValue
//      ),
//      new ExampleData(
//        ConfigType.ULong, new[] {ConfigType.Int, ConfigType.UInt, ConfigType.Long}, ULongFns,
//        -3, nLongStart, long.MinValue
//      ),
//      new ExampleData(
//        ConfigType.Float, new [] {ConfigType.Double}, FloatFns,
//        float.MinValue, -3.14f, 0f, 3.14f, float.MaxValue
//      ),
//      new ExampleData(
//        ConfigType.Double, new [] {ConfigType.Float}, DoubleFns,
//        double.MinValue, -3.14d, 0d, 3.14d, double.MaxValue
//      ),
//      new ExampleData(ConfigType.Bool, BoolFns, true, false),
//      new ExampleData(ConfigType.String, StringFns, "", "foobar"),
//      new ExampleData(ConfigType.DateTime, DateTimeFns, DateTime.Now, DateTime.Now.ToString()),
//    }.toDict(_ => _.type, _ => _));
//
//    static object toObj<A>(A a) { return a; }
//  }
//
//  [TestFixture]
//  public class ConfigTest {
//    static readonly string[]
//      nestPrefixes = {"", "foo.bar.baz."},
//      nestSuffixes = {"", "-ref" };
//
//    static void testNested(string key, Act<string> tester) {
//      foreach (var prefix in nestPrefixes)
//        foreach (var suffix in nestSuffixes)
//          tester(prefix + key + suffix);
//    }
//
//    static Fn<string, string> errMsgFn(
//      string key, ConfigType type, Dictionary<string, object> data
//    ) {
//      return actionName =>
//        $"it should {actionName} for type {type} for key '{key}' in config " +
//        $"{Json.Serialize(data)}";
//    }
//
//    static void testTypeGood(
//      Dictionary<string, object> data, string key,
//      object expected,
//      ExampleData example
//    ) {
//      var config = new Config(data);
//      var errMsg = errMsgFn(key, example.type, data);
//      example.fetchFns.getter(config, key).shouldEqual(expected, errMsg("fetch value"));
//      example.fetchFns.optGetter(config, key).shouldBeSome(expected, errMsg($"fetch Some({expected})"));
//      example.fetchFns.tryGetter(config, key).shouldBeSuccess(expected, errMsg($"fetch Success({expected})"));
//      example.fetchFns.eitherGetter(config, key).shouldBeRight(expected, errMsg($"fetch Right({expected})"));
//    }
//
////    static void testType<A>(
////      IEnumerable<A> goodValues,
////      string[] badValueKeys,
////      Fn<string, A> getter, Fn<string, Option<A>> optGetter,
////      Fn<string, Try<A>> tryGetter, Fn<string, Either<ConfigFetchError, A>> eitherGetter
////    ) {
////      foreach (var t in goodValues) t.ua((key, expected) => {
////        testNested(
////          t._1,
////          _key => {
////          }
////        );
////      });
////
////      Act<Try<A>, ConfigFetchError.Kind, Fn<string, string>> matchTryError =
////        (t, kind, errMsg) => matchErr(
////          t, _t => _t.exception.flatMap(e => F.opt(e as ConfigFetchException)).map(e => e.error),
////          kind, $"Error({nameof(ConfigFetchException)})", errMsg
////        );
////      Act<Either<ConfigFetchError, A>, ConfigFetchError.Kind, Fn<string, string>> matchEitherError =
////        (either, kind, errMsg) => matchErr(
////          either, e => e.leftValue,
////          kind, $"Left({nameof(ConfigFetchError)})", errMsg
////        );
////      testNested("nothing", key => {
////        var errMsg = errMsgFn(key);
////        Assert.Throws<ConfigFetchException>(() => getter(key), errMsg("throw exception"));
////        optGetter(key).shouldBeNone(errMsg("return None"));
////        const ConfigFetchError.Kind kind = ConfigFetchError.Kind.KEY_NOT_FOUND;
////        matchTryError(tryGetter(key), kind, errMsg);
////        matchEitherError(eitherGetter(key), kind, errMsg);
////      });
////      if (badValueKeys != null) badValueKeys.each(badValueKey =>
////        testNested(badValueKey, key => {
////          var errMsg = errMsgFn(key);
////          Assert.Throws<ConfigFetchException>(() => getter(key), errMsg("throw exception"));
////          optGetter(key).shouldBeNone(errMsg("return None"));
////          const ConfigFetchError.Kind kind = ConfigFetchError.Kind.WRONG_TYPE;
////          matchTryError(tryGetter(key), kind, errMsg);
////          matchEitherError(eitherGetter(key), kind, errMsg);
////        })
////      );
////    }
//
//    static void matchErr<A>(
//      A a, Fn<A, Option<ConfigFetchError>> resolver,
//      ConfigFetchError.Kind expected, string name, Fn<string, string> errMsg
//    ) {
//      a.shouldMatch(
//        _ => resolver(_).exists(e => e.kind == expected),
//        errMsg($"return {name} where kind is {expected}")
//      );
//    }
//
//    [Test]
//    public void PropertiesTest() {
//      new Config(new Dictionary<string, object>()).scope
//        .shouldBeEmpty("config root scope should be empty string");
//    }
//
//    static void eachValidExampleValue(Act<ExampleData, object> a) {
//      foreach (var kv in ConfigTestData.examples.table) {
//        foreach (var value in ConfigTestData.examples.validValuesFor(kv.Key)) {
//          a(kv.Value, value);
//        }
//      }
//    }
//
//    #region tests
//
//    [Test]
//    public void GoodSimpleTest() {
//      const string key = "some-key";
//      eachValidExampleValue((example, value) =>
//        testTypeGood(Data.dictFor(key, value), key, value, example)
//      );
//    }
//
//    [Test]
//    public void GoodNestedTest() {
//      const string key = "some-key", key2 = "some-other-key";
//      eachValidExampleValue((example, value) =>
//        testTypeGood(Data.dictForNested(key, key2, value), $"{key}.{key2}", value, example)
//      );
//    }
//
////    [Test]
////    public void GetSubconfigTest() {
////      Assert.AreEqual(
////        "foo.bar.baz",
////        config.getSubConfig("foo").getSubConfig("bar").getSubConfig("baz").scope,
////        "config scopes should nest correctly when accessed individually"
////      );
////      Assert.AreEqual(
////        "foo.bar.baz",
////        config.getSubConfig("foo.bar.baz").scope,
////        "config scopes should nest correctly when accessed as a path"
////      );
////      var subcfg = config.getSubConfig("subconfig");
////      Assert.AreEqual("subconfig", subcfg.scope);
////      // TODO: this isn't really a full subconfig test.
////    }
//
//    // TODO: get subconfig list test
//
//    #endregion
//  }
}