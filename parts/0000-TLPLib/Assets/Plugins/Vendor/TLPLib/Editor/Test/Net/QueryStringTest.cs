using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Net;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Test.Net {
  public class QueryStringTestParseKV {
    [Test]
    public void WhenNoEncodedParams() {
      QueryString.parseKV("foo=bar&bar=baz").shouldEqual(ImmutableList.Create(
        F.t("foo", "bar"), F.t("bar", "baz")
      ));
    }

    [Test]
    public void WhenEncodedParams() {
      QueryString.parseKV("f%20oo=b%20ar&bar=baz").shouldEqual(ImmutableList.Create(
        F.t("f oo", "b ar"), F.t("bar", "baz")
      ));
    }

    [Test]
    public void WhenPartiallyNoValue() {
      QueryString.parseKV("f%20oo&bar=baz").shouldEqual(ImmutableList.Create(
        F.t("f oo", ""), F.t("bar", "baz")
      ));
    }

    [Test]
    public void WhenNotKV() {
      QueryString.parseKV("foo").shouldEqual(ImmutableList.Create(F.t("foo", "")));
    }

    [Test]
    public void WhenEmpty() {
      QueryString.parseKV("").shouldEqual(ImmutableList<Tpl<string, string>>.Empty);
    }
  }
}