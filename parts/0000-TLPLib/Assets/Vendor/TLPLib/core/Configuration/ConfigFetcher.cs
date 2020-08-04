using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using pzd.lib.concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;
using pzd.lib.log;
using pzd.lib.serialization;
using UnityEngine;

//obsolete WWW
#pragma warning disable 618

namespace com.tinylabproductions.TLPLib.Configuration {
  public static class ConfigFetcher {
    public struct UrlWithContext : IEquatable<UrlWithContext> {
      // C# calls URLs URIs. See http://stackoverflow.com/a/1984225/935259 for distinction.
      public readonly Uri url;

      /// <summary>
      /// <see cref="LogEntry.tags"/> and <see cref="LogEntry.extras"/>
      /// </summary>
      public readonly ImmutableArray<KeyValuePair<string, string>> tags, extras;

      public UrlWithContext(
        Uri url,
        ImmutableArray<KeyValuePair<string, string>> tags,
        ImmutableArray<KeyValuePair<string, string>> extras
      ) {
        this.url = url;
        this.tags = tags;
        this.extras = extras;
      }

      public override string ToString() =>
        $"{nameof(ConfigFetcher)}.{nameof(UrlWithContext)}[" +
        $"{nameof(url)}={url}, " +
        $"{nameof(tags)}={tags.mkStringEnum()}, " +
        $"{nameof(extras)}={extras.mkStringEnum()}" +
        $"]";


      public bool Equals(UrlWithContext other) =>
        Equals(url, other.url) && tags.SequenceEqual(other.tags) && extras.SequenceEqual(other.extras);

      #region Equality

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is UrlWithContext && Equals((UrlWithContext) obj);
      }

      public override int GetHashCode() {
        unchecked {
          var hashCode = (url != null ? url.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ tags.GetHashCode();
          hashCode = (hashCode * 397) ^ extras.GetHashCode();
          return hashCode;
        }
      }

      public static bool operator ==(UrlWithContext left, UrlWithContext right) { return left.Equals(right); }
      public static bool operator !=(UrlWithContext left, UrlWithContext right) { return !left.Equals(right); }

      #endregion

      public static readonly ISerializedRW<UrlWithContext> serializedRW =
        SerializedRW.uri.and(
          LogEntry.stringTupleArraySerializedRw, LogEntry.stringTupleArraySerializedRw,
          (url, tags, extras) => new UrlWithContext(url, tags, extras),
          url => url.url,
          url => url.tags,
          url => url.extras
        );
    }

    public static Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> fetch(
      UrlWithContext urls
    ) =>
      F.t(
        urls,
        new WWW(urls.url.ToString()).toFuture().asNonCancellable().map(wwwE => {
          var www = wwwE.fold(err => err.www, _ => _);
          var headers = www.headers();
          return wwwE
            .mapLeft(err => (ConfigFetchError)new ConfigWWWError(urls, headers))
            .mapRight(_ => headers);
        })
      );

    public static Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> withTimeout(
      this Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> tpl,
      Duration timeout, ITimeContext timeContext
    ) =>
      tpl.map2((urls, future) =>
        future
          .timeout(timeout, () => (ConfigFetchError) new ConfigTimeoutError(urls, timeout), timeContext)
          .map(e => e.flatMapRight(_ => _))
      );

    public static Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> checkingServerHeader(
      this Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> tpl,
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
              : Either<ConfigFetchError, WWWWithHeaders>.Left(new ConfigHeaderCheckFailed(
                urls, headerName, expectedValue, actual
              ));
          }
        );
      })
    );

    public static Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> checkingContentType(
      this Tpl<UrlWithContext, Future<Either<ConfigFetchError, WWWWithHeaders>>> tpl,
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