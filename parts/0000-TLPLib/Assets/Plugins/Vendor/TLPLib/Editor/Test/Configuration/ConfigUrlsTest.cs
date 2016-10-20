using System;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Configuration {
  public class ConfigUrlsTest {
    [Test]
    public void TestToString() {
      var urls = new Config.Urls(new Uri("http://fetch.url"), new Uri("http://report.url"));
      var str = urls.ToString();
      str.shouldInclude(urls.reportUrl.ToString());
      str.shouldNotInclude(urls.fetchUrl.ToString());
    }
  }
}