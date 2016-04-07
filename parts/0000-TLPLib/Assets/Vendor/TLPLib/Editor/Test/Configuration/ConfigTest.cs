using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Formats.SimpleJSON;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Configuration {
  static class TestJson {
    public static string json(this string json) { return json.Replace('\'', '"'); }
  }

  [TestFixture]
  public class ConfigTest {
    private readonly static string json =
@"{
  'foo': {
    'bar': {
      'baz': {
        'str': 'string',
        'str-list': ['s1', 's2'],
        'int': 55100,
        'int-list': [1,2,3],
        'float': 35.53,
        'float-list': [1.5, 2.5, 3.5],
        'bool': true,
        'bool-list': [true, false],

        'str-ref': '#REF=foo.bar.baz.str#',
        'str-list-ref': '#REF=foo.bar.baz.str-list#',
        'int-ref': '#REF=foo.bar.baz.int#',
        'int-list-ref': '#REF=foo.bar.baz.int-list#',
        'float-ref': '#REF=foo.bar.baz.float#',
        'float-list-ref': '#REF=foo.bar.baz.float-list#',
        'bool-ref': '#REF=foo.bar.baz.bool#',
        'bool-list-ref': '#REF=foo.bar.baz.bool-list#',
        'subconfig-ref': '#REF=foo.bar.baz#',
        'subconfig-list-ref': '#REF=subconfig-list#'
      }
    },
  },
  'subconfig': {
    'str': 'string',
    'str-list': ['s1', 's2'],
    'int': 55100,
    'int-list': [1,2,3],
    'float': 35.53,
    'float-list': [1.5, 2.5, 3.5],
    'bool': true,
    'bool-list': [true, false]
  },
  'subconfig-list': [
    {
      'str': 'string',
      'str-list': ['s1', 's2'],
      'int': 55100,
      'int-list': [1,2,3],
      'float': 35.53,
      'float-list': [1.5, 2.5, 3.5],
      'bool': true,
      'bool-list': [true, false]
    }, 
    {
      'str': 'string',
      'str-list': ['s1', 's2'],
      'int': 55100,
      'int-list': [1,2,3],
      'float': 35.53,
      'float-list': [1.5, 2.5, 3.5],
      'bool': true,
      'bool-list': [true, false]
    }
  ],

  'str': 'string',
  'str-list': ['s1', 's2'],
  'int': 55100,
  'int-list': [1,2,3],
  'float': 35.53,
  'float-list': [1.5, 2.5, 3.5],
  'bool': true,
  'bool-list': [true, false],

  'str-ref': '#REF=foo.bar.baz.str#',
  'str-list-ref': '#REF=foo.bar.baz.str-list#',
  'int-ref': '#REF=foo.bar.baz.int#',
  'int-list-ref': '#REF=foo.bar.baz.int-list#',
  'float-ref': '#REF=foo.bar.baz.float#',
  'float-list-ref': '#REF=foo.bar.baz.float-list#',
  'bool-ref': '#REF=foo.bar.baz.bool#',
  'bool-list-ref': '#REF=foo.bar.baz.bool-list#',
  'subconfig-ref': '#REF=foo.bar.baz#',
  'subconfig-list-ref': '#REF=subconfig-list#'
}".json();
    private static readonly IConfig config = new Config(
      (Dictionary<string, object>) Json.Deserialize(json)
    );
    private static readonly string[] nestPrefixes = {"", "foo.bar.baz."};
    private static readonly string[] nestSuffixes = {"", "-ref"};

    private static void testNested(string key, Act<string> tester) {
      foreach (var prefix in nestPrefixes) 
        foreach (var suffix in nestSuffixes) 
          tester(prefix + key + suffix);
    }

    private static void testGetter<A>(
      string goodValueKey, A goodValue, string badValueKey, 
      Fn<string, A> fetcher
    ) {
      testNested(goodValueKey, key => Assert.AreEqual(
        goodValue, fetcher(key), "it should fetch value for " + key
      ));
      testNested("nothing", key => Assert.Throws<ConfigFetchException>(
        () => fetcher(key),
        "it should throw exception on non-existant value for " + key
      ));
      if (badValueKey != null)
        testNested(badValueKey, key => Assert.Throws<ConfigFetchException>(
          () => fetcher(key),
          "it should throw exception on wrong type for " + key
        ));
    }

    private static void testOpt<A>(
      string goodValueKey, A goodValue, string badValueKey, 
      Fn<string, Option<A>> fetcher
    ) {
      testNested(goodValueKey, key => {
        var value = fetcher(key);
        Assert.True(value.isDefined, "it should return Some(value) for " + key);
        Assert.AreEqual(
          goodValue, value.get,
          "it should return value for " + key
        );
      });
      testNested("nothing", key => Assert.AreEqual(
        F.none<A>(), fetcher(key),
        "it should return None on non-existant value for " + key
      ));
      if (badValueKey != null)
        testNested(badValueKey, key => Assert.AreEqual(
          F.none<A>(), fetcher(key),
          "it should return None on wrong type for " + key
        ));
    }

    private static void testEither<A>(
      string goodValueKey, A goodValue, string badValueKey,
      Fn<string, Either<ConfigFetchError, A>> fetcher
    ) {
      testNested(goodValueKey, key => {
        var value = fetcher(key);
        Assert.True(value.isRight, "it should return Right(value) for " + key);
        Assert.AreEqual(
          goodValue, value.rightValue.get,
          "it should return value for " + key
        );
      });
      testNested("nothing", key => Assert.True(
        fetcher(key).isLeft,
        "it should return Left(error) on non-existant value for " + key
      ));
      if (badValueKey != null)
        testNested(badValueKey, key => Assert.True(
          fetcher(key).isLeft,
          "it should return Left(error) on wrong type for " + key
        ));
    }

    private static void testTry<A>(
      string goodValueKey, A goodValue, string badValueKey,
      Fn<string, Try<A>> fetcher
    ) {
      testNested(goodValueKey, key => {
        var value = fetcher(key);
        Assert.True(value.isSuccess, "it should return Success(value) for " + key);
        Assert.AreEqual(
          goodValue, value.getOrThrow,
          "it should return value for " + key
        );
      });
      testNested("nothing", key => Assert.True(
        fetcher(key).isError,
        "it should return Error(error) on non-existant value for " + key
      ));
      if (badValueKey != null)
        testNested(badValueKey, key => Assert.True(
          fetcher(key).isError,
          "it should return Error(error) on wrong type for " + key
        ));
    }

    [Test] public void PropertiesTest() {
      Assert.AreEqual("", config.scope, "config root scope should be empty string");
    }

    #region getters

    [Test] public void GetStringTest() 
    { testGetter("str", "string", null, config.getString); }

    [Test] public void GetStringListTest() 
    { testGetter("str-list", F.ilist("s1", "s2"), null, config.getStringList); }

    [Test] public void GetIntTest() 
    { testGetter("int", 55100, "str", config.getInt); }

    [Test] public void GetIntListTest() 
    { testGetter("int-list", F.ilist(1,2,3), "str-list", config.getIntList); }

    [Test] public void GetFloatTest() 
    { testGetter("float", 35.53f, "str", config.getFloat); }

    [Test] public void GetFloatListTest() 
    { testGetter("float-list", F.ilist(1.5f, 2.5f, 3.5f), "str-list", config.getFloatList); }

    [Test] public void GetBoolTest() 
    { testGetter("bool", true, "str", config.getBool); }

    [Test] public void GetBoolListTest() 
    { testGetter("bool-list", F.ilist(true, false), "str-list", config.getBoolList); }

    [Test]
    public void GetSubconfigTest() {
      Assert.AreEqual(
        "foo.bar.baz", 
        config.getSubConfig("foo").getSubConfig("bar").getSubConfig("baz").scope,
        "config scopes should nest correctly when accessed individually"
      );
      Assert.AreEqual(
        "foo.bar.baz", 
        config.getSubConfig("foo.bar.baz").scope,
        "config scopes should nest correctly when accessed as a path"
      );
      var subcfg = config.getSubConfig("subconfig");
      Assert.AreEqual("subconfig", subcfg.scope);
      // TODO: this isn't really a full subconfig test.
    }

    // TODO: get subconfig list test

    #endregion

    #region opt getters

    [Test] public void OptStringTest() 
    { testOpt("str", "string", null, k => config.optString(k)); }

    [Test] public void OptStringListTest() 
    { testOpt("str-list", F.ilist("s1", "s2"), null, k => config.optStringList(k)); }

    [Test] public void OptIntTest() 
    { testOpt("int", 55100, "str", k => config.optInt(k)); }

    [Test] public void OptIntListTest() 
    { testOpt("int-list", F.ilist(1, 2, 3), "str-list", k => config.optIntList(k)); }

    [Test] public void OptFloatTest() 
    { testOpt("float", 35.53f, "str", k => config.optFloat(k)); }

    [Test] public void OptFloatListTest() 
    { testOpt("float-list", F.ilist(1.5f, 2.5f, 3.5f), "str-list", k => config.optFloatList(k)); }

    [Test] public void OptBoolTest() 
    { testOpt("bool", true, "str", k => config.optBool(k)); }

    [Test] public void OptBoolListTest() 
    { testOpt("bool-list", F.ilist(true, false), "str-list", k => config.optBoolList(k)); }

    #endregion

    #region either getters

    [Test] public void EitherStringTest() 
    { testEither("str", "string", null, k => config.eitherString(k)); }

    [Test] public void EitherStringListTest() 
    { testEither("str-list", F.ilist("s1", "s2"), null, k => config.eitherStringList(k)); }

    [Test] public void EitherIntTest() 
    { testEither("int", 55100, "str", k => config.eitherInt(k)); }

    [Test] public void EitherIntListTest() 
    { testEither("int-list", F.ilist(1, 2, 3), "str-list", k => config.eitherIntList(k)); }

    [Test] public void EitherFloatTest() 
    { testEither("float", 35.53f, "str", k => config.eitherFloat(k)); }

    [Test] public void EitherFloatListTest() 
    { testEither("float-list", F.ilist(1.5f, 2.5f, 3.5f), "str-list", k => config.eitherFloatList(k)); }

    [Test] public void EitherBoolTest() 
    { testEither("bool", true, "str", k => config.eitherBool(k)); }

    [Test] public void EitherBoolListTest() 
    { testEither("bool-list", F.ilist(true, false), "str-list", k => config.eitherBoolList(k)); }

    #endregion

    #region try getters

    [Test] public void TryStringTest() 
    { testTry("str", "string", null, k => config.tryString(k)); }

    [Test] public void TryStringListTest() 
    { testTry("str-list", F.ilist("s1", "s2"), null, k => config.tryStringList(k)); }

    [Test] public void TryIntTest() 
    { testTry("int", 55100, "str", k => config.tryInt(k)); }

    [Test] public void TryIntListTest() 
    { testTry("int-list", F.ilist(1, 2, 3), "str-list", k => config.tryIntList(k)); }

    [Test] public void TryFloatTest() 
    { testTry("float", 35.53f, "str", k => config.tryFloat(k)); }

    [Test] public void TryFloatListTest() 
    { testTry("float-list", F.ilist(1.5f, 2.5f, 3.5f), "str-list", k => config.tryFloatList(k)); }

    [Test] public void TryBoolTest() 
    { testTry("bool", true, "str", k => config.tryBool(k)); }

    [Test] public void TryBoolListTest() 
    { testTry("bool-list", F.ilist(true, false), "str-list", k => config.tryBoolList(k)); }

    #endregion
  }
}