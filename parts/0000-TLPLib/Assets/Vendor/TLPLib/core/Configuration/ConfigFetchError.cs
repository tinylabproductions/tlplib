using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Configuration {
  public abstract class ConfigFetchError {
    public readonly ConfigFetcher.UrlWithContext url;
    public readonly string message;

    protected ConfigFetchError(ConfigFetcher.UrlWithContext url, string message) {
      this.message = message;
      this.url = url;
    }

    public override string ToString() => $"{nameof(ConfigFetchError)}[{url}, {message}]";
  }

  public class ConfigTimeoutError : ConfigFetchError {
    public readonly Duration timeout;

    public ConfigTimeoutError(ConfigFetcher.UrlWithContext url, Duration timeout)
    : base(url, $"Timed out: {timeout}")
    { this.timeout = timeout; }
  }

  public class ConfigHeaderCheckFailed : ConfigFetchError {
    public ConfigHeaderCheckFailed(
      ConfigFetcher.UrlWithContext url, string headerName, string expectedValue, Option<string> actual
    ) : base(
      url, $"Expected header '{headerName}' to be '{expectedValue}', but it was {actual}"
    ) {}
  }

  public class ConfigWWWError : ConfigFetchError {
    public readonly WWWWithHeaders wwwWithHeaders;

    public ConfigWWWError(ConfigFetcher.UrlWithContext url, WWWWithHeaders wwwWithHeaders)
    : base(url, $"WWW error: {wwwWithHeaders.www.error}")
    { this.wwwWithHeaders = wwwWithHeaders; }
  }

  public class ConfigWrongContentType : ConfigHeaderCheckFailed {
    public ConfigWrongContentType(ConfigFetcher.UrlWithContext url, string expected, string actual)
    : base(url, "Content-Type", expected, actual.some()) {}
  }
}