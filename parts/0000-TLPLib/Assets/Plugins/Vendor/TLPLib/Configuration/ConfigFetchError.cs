using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Configuration {
  public abstract class ConfigFetchError {
    public readonly ConfigFetcher.Urls urls;
    public readonly string message;

    protected ConfigFetchError(ConfigFetcher.Urls urls, string message) {
      this.message = message;
      this.urls = urls;
    }

    public override string ToString() => $"{nameof(ConfigFetchError)}[{urls}, {message}]";
  }

  public class ConfigTimeoutError : ConfigFetchError {
    public readonly Duration timeout;

    public ConfigTimeoutError(ConfigFetcher.Urls urls, Duration timeout)
    : base(urls, $"Timed out: {timeout}")
    { this.timeout = timeout; }
  }

  public class ConfigServerCheckFailed : ConfigFetchError {
    public ConfigServerCheckFailed(
      ConfigFetcher.Urls urls, string headerName, string expectedValue, Option<string> actual
    ) : base(
      urls, $"Expected header '{headerName}' to be '{expectedValue}', but it was {actual}"
    ) {}
  }

  public class ConfigWWWError : ConfigFetchError {
    public readonly WWWWithHeaders wwwWithHeaders;

    public ConfigWWWError(ConfigFetcher.Urls urls, WWWWithHeaders wwwWithHeaders)
    : base(urls, $"WWW error: {wwwWithHeaders.www.error}")
    { this.wwwWithHeaders = wwwWithHeaders; }
  }

  public class ConfigWrongContentType : ConfigFetchError {
    public readonly string expectedContentType, actualContentType;

    public ConfigWrongContentType(ConfigFetcher.Urls urls, string expectedContentType, string actualContentType)
    : base(
      urls, $"Expected 'Content-Type' to be '{expectedContentType}', but it was '{actualContentType}'"
    ) {
      this.expectedContentType = expectedContentType;
      this.actualContentType = actualContentType;
    }
  }
}