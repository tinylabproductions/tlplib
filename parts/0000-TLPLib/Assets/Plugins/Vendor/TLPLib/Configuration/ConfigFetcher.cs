using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Configuration {
  public static class ConfigFetcher {
    public struct Urls : IEquatable<Urls> {
      // C# calls URLs URIs. See http://stackoverflow.com/a/1984225/935259 for distinction.
      /** Actual URL this config needs to be fetched. **/
      public readonly Uri fetchUrl;
      /**
       * URL used in reporting. For example you might want to not
       * include timestamp when sending the URL to your error logger.
       **/
      public readonly Uri reportUrl;

      public Urls(Uri fetchUrl) : this(fetchUrl, fetchUrl) {}

      public Urls(Uri fetchUrl, Uri reportUrl) {
        this.fetchUrl = fetchUrl;
        this.reportUrl = reportUrl;
      }

      public override string ToString() =>
        $"{nameof(Config)}.{nameof(Urls)}[{reportUrl}]";

      #region Equality

      public bool Equals(Urls other) {
        return Equals(fetchUrl, other.fetchUrl) && Equals(reportUrl, other.reportUrl);
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Urls && Equals((Urls) obj);
      }

      public override int GetHashCode() {
        unchecked { return ((fetchUrl != null ? fetchUrl.GetHashCode() : 0) * 397) ^ (reportUrl != null ? reportUrl.GetHashCode() : 0); }
      }

      public static bool operator ==(Urls left, Urls right) { return left.Equals(right); }
      public static bool operator !=(Urls left, Urls right) { return !left.Equals(right); }

      #endregion

      public static readonly ISerializedRW<Urls> serializedRW =
        SerializedRW.uri.and(SerializedRW.uri).map(
          t => F.some(new Urls(t._1, t._2)),
          urls => F.t(urls.fetchUrl, urls.reportUrl)
        );
    }

    public static Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> fetch(
      Urls urls
    ) => 
      F.t(
        urls,
        new WWW(urls.fetchUrl.ToString()).wwwFuture().asNonCancellable().map(wwwE => {
          var www = wwwE.fold(err => err.www, _ => _);
          var headers = www.headers();
          return wwwE.map(
            err => (ConfigFetchError)new ConfigWWWError(urls, headers), 
            _ => headers
          );
        })
      );

    public static Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> withTimeout(
      this Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> tpl,
      Duration timeout, ITimeContext timeContext = default(ITimeContext)
    ) {
      timeContext = timeContext.orDefault();
      return tpl.map2((urls, future) => 
        future
        .timeout(timeout, () => (ConfigFetchError) new ConfigTimeoutError(urls, timeout), timeContext)
        .map(e => e.flatten())
      );
    }

    public static Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> checkingServerHeader(
      this Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> tpl,
      string headerName, string expectedValue
    ) => tpl.map2((urls, future) => 
      future.map(wwwE => {
        var headersOpt = wwwE.fold(
          err => F.opt(err as ConfigWWWError).map(_ => _.wwwWithHeaders),
          _ => _.some()
        );
        return headersOpt.fold(
          wwwE,
          headers => {
            var actual = headers[headerName];
            return actual.exists(expectedValue)
              ? wwwE
              : Either<ConfigFetchError, WWWWithHeaders>.Left(new ConfigServerCheckFailed(
                urls, headerName, expectedValue, actual
              ));
          }
        );
      })
    );

    public static Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> checkingContentType(
      this Tpl<Urls, Future<Either<ConfigFetchError, WWWWithHeaders>>> tpl,
      string expectedContentType = "application/json"
    ) => tpl.map2((urls, future) =>
      future.map(wwwE => wwwE.flatMapRight(headers => {
        var contentType = headers["CONTENT-TYPE"].getOrElse("undefined");
        // Sometimes we get redirected to internet paygate, which returns HTML
        // instead of our content.
        if (contentType != expectedContentType)
          return Either<ConfigFetchError, WWWWithHeaders>.Left(
            new ConfigWrongContentType(urls, expectedContentType, contentType)
          );

        return wwwE;
      }))
    );

    public static Future<Either<ConfigFetchError, string>> content(
      this Future<Either<ConfigFetchError, WWWWithHeaders>> future
    ) => future.map(e => e.mapRight(t => t.www.text));
  }
}