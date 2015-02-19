#if UNITY_TEST
using System.Linq;
using com.tinylabproductions.TLPLib.Formats.SimpleJSON;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Configuration.Editor {
  [TestFixture]
  public class VariableConfigTest {
    [Test]
    public void InjectToKeySingleTest() {
      Assert.AreEqual(
        new []{"a.key", "b.key"},
        VariableConfig.injectToKey("key", new []{"a", "b"})
      );
      Assert.AreEqual(
        new []{"some.a.key", "some.b.key"},
        VariableConfig.injectToKey("some.key", new []{"a", "b"})
      );
      Assert.AreEqual(
        new []{"some.long.a.key", "some.long.b.key"},
        VariableConfig.injectToKey("some.long.key", new []{"a", "b"})
      );
    }

    [Test]
    public void InjectToKeyVariationsTest() {
      Assert.AreEqual(
        new [] {"some.key"},
        VariableConfig.injectToKey(
          "some.key", 
          new string[0][],
          F.dict(F.t("a", "a-value"), F.t("b", "b-value"), F.t("c", "c-value"))
        ).ToArray()
      );
      Assert.AreEqual(
        new [] {
          "some.a-value.b-value.key", "some.b-value.c-value.key",
          "some.c-value.b-value.a-value.key", "some.key"
        },
        VariableConfig.injectToKey(
          "some.key", 
          new []{new []{"a", "b"}, new []{"b", "c"}, new []{"c", "b", "a"}}, 
          F.dict(F.t("a", "a-value"), F.t("b", "b-value"), F.t("c", "c-value"))
        ).ToArray()
      );
    }

    [Test]
    public void VariableConfigNestedTest() {
      var json =
@"{
  'a-var': {
    'b-var': {
      'str': 'string-a-b'
    },
    'str': 'string-a',
    'str2': 'string2-a'
  },
  'str': 'string',
  'str2': 'string2',
  'str3': 'string3'
}".json();
      var variables = F.dict(F.t("a", "a-var"), F.t("b", "b-var"));
      var cfg = new Config(JSON.Parse(json).AsObject);

      var cfgab = new VariableConfig(
        cfg, variables, new[] {new[] {"a", "b"}, new[] {"a"}}
      );
      Assert.AreEqual("string-a-b", cfgab.getString("str"));
      Assert.AreEqual("string2-a", cfgab.getString("str2"));
      Assert.AreEqual("string3", cfgab.getString("str3"));

      var cfga = new VariableConfig(
        cfg, variables, new[] {new[] {"a"}}
      );
      Assert.AreEqual("string-a", cfga.getString("str"));
      Assert.AreEqual("string2-a", cfga.getString("str2"));
      Assert.AreEqual("string3", cfga.getString("str3"));

      var cfgb = new VariableConfig(
        cfg, variables, new[] {new[] {"b"}}
      );
      Assert.AreEqual("string", cfgb.getString("str"));
      Assert.AreEqual("string2", cfgb.getString("str2"));
      Assert.AreEqual("string3", cfgb.getString("str3"));

      var cfgNone = new VariableConfig(
        cfg, variables, new[] {new string[] {}}
      );
      Assert.AreEqual("string", cfgNone.getString("str"));
      Assert.AreEqual("string2", cfgNone.getString("str2"));
      Assert.AreEqual("string3", cfgNone.getString("str3"));
    }

    [Test]
    public void InjectedSubconfigsTest() {
      var json =
@"{
  'networks': [{'name': 'admob'}],
  'wp8': {
    'networks': [{'name': 'revmob'}],
  }
}".json();
      var variables = F.dict(F.t("platform", "wp8"));
      var cfg = new Config(JSON.Parse(json).AsObject);

      var plainCfg = new VariableConfig(cfg, variables, new[] { new string[] { } });
      var wp8Cfg = new VariableConfig(cfg, variables, new[] {new[] {"platform"}});
      Assert.AreEqual(
        "admob",
        plainCfg.getSubConfigList("networks")[0].getString("name")
      );
      Assert.AreEqual(
        "revmob",
        wp8Cfg.getSubConfigList("networks")[0].getString("name")
      );
    }
  }
}
#endif