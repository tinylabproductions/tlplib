﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.SimpleJSON;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Configuration {
  /**
   * Config class that fetches JSON configuration from `url`. Contents of 
   * `url` are expected to be a JSON object.
   * 
   * It takes keys in format of `key.subkey.subsubkey`. It as well tries to 
   * get platform specific overrides in following order:
   * 
   * - `key.subkey.{Platform.fullName}.subsubkey`
   * - `key.subkey.{Platform.name}.subsubkey`
   * - `key.subkey.subsubkey`
   * 
   * This does not work for fetching subconfigs, because a person that edits 
   * the configuration shouldn't know whether you're fetching values with
   * subconfig or not to correctly write the configuration.
   **/

  public class Config {
    public static Future<Config> apply(string url) {
      return ASync.StartCoroutine<JSONClass>(
        p => getConfiguration(url, p)
      ).map(data => new Config(url, data));
    }

    // Implementation

    private delegate Option<A> Parser<out A>(JSONNode node);

    private static readonly Parser<string> stringParser = n => F.some(n.Value);
    private static readonly Parser<int> intParser = n => n.Value.parseInt();
    private static readonly Parser<float> floatParser = n => n.Value.parseFloat();
    private static readonly Parser<bool> boolParser = n => n.Value.parseBool();

    public readonly string url;
    private readonly JSONClass configuration;

    private Config(string url, JSONClass configuration) {
      this.url = url;
      this.configuration = configuration;
    }

    #region getters

    public string getString(string key) { return tryString(key).getOrThrow; }
    public IList<string> getStringList(string key) 
      { return tryStringList(key).getOrThrow; }
    public int getInt(string key) { return tryInt(key).getOrThrow; }
    public IList<int> getIntList(string key) 
      { return tryIntList(key).getOrThrow; }
    public float getFloat(string key) { return tryFloat(key).getOrThrow; }
    public IList<float> getFloatList(string key) { return tryFloatList(key).getOrThrow; }
    public bool getBool(string key) { return tryBool(key).getOrThrow; }
    public IList<bool> getBoolList(string key) { return tryBoolList(key).getOrThrow; }
    public Config getSubConfig(string key) 
      { return trySubConfig(key).getOrThrow; }

    #endregion

    #region opt getters

    public Option<string> optString(string key) {
      return get(key, stringParser).fold(_ => F.none<string>(), F.some);
    }

    public Option<IList<string>> optStringList(string key) {
      return getList(key, stringParser).
        fold(_ => F.none<IList<string>>(), F.some);
    }

    public Option<int> optInt(string key) {
      return get(key, intParser).fold(_ => F.none<int>(), F.some);
    }

    public Option<IList<int>> optIntList(string key) {
      return getList(key, intParser).
        fold(_ => F.none<IList<int>>(), F.some);
    }

    public Option<float> optFloat(string key) {
      return get(key, floatParser).fold(_ => F.none<float>(), F.some);
    }

    public Option<IList<float>> optFloatList(string key) {
      return getList(key, floatParser).
        fold(_ => F.none<IList<float>>(), F.some);
    }

    public Option<bool> optBool(string key) {
      return get(key, boolParser).fold(_ => F.none<bool>(), F.some);
    }

    public Option<IList<bool>> optBoolList(string key) {
      return getList(key, boolParser).
        fold(_ => F.none<IList<bool>>(), F.some);
    }

    public Option<Config> optSubConfig(string key) {
      return fetchSubConfig(key).fold(_ => F.none<Config>(), F.some);
    }

    #endregion

    #region try getters

    public Try<string> tryString(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return get(key, stringParser).fold<Try<string>>(tryArgEx<string>, F.scs);
    }

    public Try<IList<string>> tryStringList(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return getList(key, stringParser).
        fold<Try<IList<string>>>(tryArgEx<IList<string>>, F.scs);
    }

    public Try<int> tryInt(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return get(key, n => n.Value.parseInt()).
        fold<Try<int>>(tryArgEx<int>, F.scs);
    }

    public Try<IList<int>> tryIntList(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return getList(key, intParser).
        fold<Try<IList<int>>>(tryArgEx<IList<int>>, F.scs);
    }

    public Try<float> tryFloat(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return get(key, n => n.Value.parseFloat()).
        fold<Try<float>>(tryArgEx<float>, F.scs);
    }

    public Try<IList<float>> tryFloatList(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return getList(key, floatParser).
        fold<Try<IList<float>>>(tryArgEx<IList<float>>, F.scs);
    }

    public Try<bool> tryBool(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return get(key, n => n.Value.parseBool()).
        fold<Try<bool>>(tryArgEx<bool>, F.scs);
    }

    public Try<IList<bool>> tryBoolList(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return getList(key, boolParser).
        fold<Try<IList<bool>>>(tryArgEx<IList<bool>>, F.scs);
    }

    public Try<Config> trySubConfig(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug
      return fetchSubConfig(key).
        fold<Try<Config>>(tryArgEx<Config>, F.scs);
    }

    private static Try<A> tryArgEx<A>(string msg) {
      return F.err<A>(new ArgumentException(msg));
    }

    #endregion

    private Either<string, Config> fetchSubConfig(string key) {
      return getConcrete(split(key), n => F.opt(n.AsObject)).
        mapRight(n => new Config(url, n));
    }

    private Either<string, A> get<A>(string key, Parser<A> parser) {
      var parts = split(key);

      var fullPlatform = Platform.fullName;
      var platform = Platform.name;

      // Try getting platform + subplatform, then platform, then generic key.
      var current = getConcrete(injectPlatform(parts, fullPlatform), parser);
      current = current.flatMapLeft(_ =>
        // Do not access again if fullPlatform == platform.
        fullPlatform == platform 
          ? current : getConcrete(injectPlatform(parts, platform), parser)
      );
      current = current.flatMapLeft(__ => getConcrete(parts, parser));
      return current;
    }

    private static string[] split(string key) {
      return key.Split('.');
    }

    private static string[] injectPlatform(string[] parts, string platform) {
      var output = new string[parts.Length + 1];
      Array.Copy(parts, output, parts.Length - 1);
      output[parts.Length - 1] = platform;
      output[parts.Length] = parts[parts.Length];
      return output;
    }

    private Either<string, IList<A>> getList<A>(
      string key, Parser<A> parser
    ) {
      return getConcrete(split(key), n => F.some(n.AsArray)).
        flatMapRight(arr => arr.Childs.ZipWithIndex().Aggregate(
          F.right<string, IList<A>>(new List<A>(arr.Count)),
          // Mono compiler bug
          (state, t) => parser(t._1).fold<Either<string, IList<A>>>(
            () => F.left<string, IList<A>>(string.Format(
              "Cannot convert '{0}'[{1}] to {2}: {3}",
              key, t._2, typeof(A), t._1
            )),
            // ReSharper disable once RedundantTypeArgumentsOfMethod
            // Mono compiler bug
            item => state.mapRight<IList<A>>(list => {
              list.Add(item);
              return list;
            })
          )
        )
      );
    }

    private Either<string, A> getConcrete<A>(
      IList<string> parts, Parser<A> parser
    ) {
      var current = configuration;

      foreach (var part in parts.dropRight(1)) {
        var node = current[part];
        if (node == null) return F.left<string, A>(string.Format(
          "Cannot find part '{0}' from key '{1}' in {2}",
          part, parts.mkString("."), current
        ));
        var obj = node.AsObject;
        if (obj == null) return F.left<string, A>(string.Format(
          "Cannot convert part '{0}' from key '{1}' to js object. Contents: {2}",
          part, parts.mkString("."), current
        ));
        current = obj;
      }

      var lastPart = parts.lastOpt().get;
      var converted = parser(current[lastPart]);
      return converted.fold(() => F.left<string, A>(string.Format(
        "Cannot convert part '{0}' from key '{1}' to '{2}'. Contents: {3}",
        lastPart, parts.mkString("."), typeof(A), current
      )), F.right<string, A>);
    }

    private static IEnumerator getConfiguration(
      string url, Promise<JSONClass> promise
    ) {
      var req = new WWW(url);
      yield return req;

      if (! string.IsNullOrEmpty(req.error)) {
        promise.completeError(new Exception(string.Format(
          "Can't load {0}: {1}", url, req.error
        )));
      }
      else {
        try {
          var json = JSON.Parse(req.text).AsObject;
          if (json == null)
            promise.completeError(new Exception(string.Format(
              "Cannot parse url '{0}' contents as JSON object:\n{1}", 
              url, req.text
            )));
          else 
            promise.completeSuccess(json);
        }
        catch (Exception e) { promise.completeError(e); }
      }
    }
  }
}
