using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.SimpleJSON;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Configuration {
  /* See IConfig. */
  public class Config : ConfigBase {
    public class ConfigError : Exception {
      public ConfigError(string message) : base(message) {}
    }

    /** Errors which happen because retrieval fails. */
    public class ConfigRetrievalError : ConfigError {
      public ConfigRetrievalError(string message) : base(message) {}
    }

    public class WrongContentType : ConfigRetrievalError {
      public readonly string url, expectedContentType, actualContentType;

      public WrongContentType(string url, string expectedContentType, string actualContentType) 
      : base(
        $"Expected 'Content-Type' in '{url}' to be '{expectedContentType}', but it was '{actualContentType}'"
      ) {
        this.url = url;
        this.expectedContentType = expectedContentType;
        this.actualContentType = actualContentType;
      }
    }

    /** Errors which happen because the developers screwed up config content. */
    public class ConfigContentError : ConfigError {
      public ConfigContentError(string message) : base(message) {}
    }

    public class ParsingError : ConfigContentError {
      public readonly string url, jsonString;

      public ParsingError(string url, string jsonString) : base(
        $"Cannot parse url '{url}' contents as JSON object:\n{jsonString}"
      ) {
        this.url = url;
        this.jsonString = jsonString;
      }
    }

    /**
     * Fetches JSON config from URL. Checks its content type before parsing.
     *
     * Throws WrongContentType if unexpected content type is found. 
     * Throws ParsingError if JSON could not be parsed,.
     **/
    public static Future<IConfig> apply(string url, string expectedContentType= "application/json") {
      return new WWW(url).wwwFuture().map(www => {
        var contentType = www.responseHeaders.get("CONTENT-TYPE").getOrElse("undefined");
        // Sometimes we get redirected to internet paygate, which returns HTML 
        // instead of our content.
        if (contentType != expectedContentType)
          throw new WrongContentType(url, expectedContentType, contentType);

        var json = JSON.Parse(www.text).AsObject;
        if (json == null) throw new ParsingError(url, www.text);
        return (IConfig) new Config(json);
      });
    }

    // Implementation

    delegate Option<A> Parser<A>(JSONNode node);

    static readonly Parser<JSONClass> jsClassParser = n => F.opt(n.AsObject);
    static readonly Parser<string> stringParser = n => F.some(n.Value);
    static readonly Parser<int> intParser = n => n.Value.parseInt().rightValue;
    static readonly Parser<float> floatParser = n => n.Value.parseFloat().rightValue;
    static readonly Parser<bool> boolParser = n => n.Value.parseBool().rightValue;
    static readonly Parser<DateTime> dateTimeParser = n => n.Value.parseDateTime().rightValue;

    public override string scope { get; }

    readonly JSONClass root, scopedRoot;

    public Config(JSONClass root, JSONClass scopedRoot=null, string scope="") {
      this.scope = scope;
      this.root = root;
      this.scopedRoot = scopedRoot ?? root;
    }

    #region either getters

    public override Either<ConfigFetchError, string> eitherString(string key) 
    { return get(key, stringParser); }

    public override Either<ConfigFetchError, IList<string>> eitherStringList(string key) 
    { return getList(key, stringParser); }

    public override Either<ConfigFetchError, int> eitherInt(string key) 
    { return get(key, intParser); }

    public override Either<ConfigFetchError, IList<int>> eitherIntList(string key) 
    { return getList(key, intParser); }

    public override Either<ConfigFetchError, float> eitherFloat(string key) 
    { return get(key, floatParser); }

    public override Either<ConfigFetchError, IList<float>> eitherFloatList(string key) 
    { return getList(key, floatParser); }

    public override Either<ConfigFetchError, bool> eitherBool(string key) 
    { return get(key, boolParser); }

    public override Either<ConfigFetchError, IList<bool>> eitherBoolList(string key) 
    { return getList(key, boolParser); }

    public override Either<ConfigFetchError, DateTime> eitherDateTime(string key) 
    { return get(key, dateTimeParser); }

    public override Either<ConfigFetchError, IList<DateTime>> eitherDateTimeList(string key) 
    { return getList(key, dateTimeParser); }

    public override Either<ConfigFetchError, IConfig> eitherSubConfig(string key) 
    { return fetchSubConfig(key); }

    public override Either<ConfigFetchError, IList<IConfig>> eitherSubConfigList(string key) 
    { return fetchSubConfigList(key); }

    #endregion

    Either<ConfigFetchError, IConfig> fetchSubConfig(string key) {
      return get(key, jsClassParser).mapRight(n => 
        (IConfig) new Config(root, n, scope == "" ? key : scope + "." + key)
      );
    }

    Either<ConfigFetchError, IList<IConfig>> fetchSubConfigList(string key) {
      return getList(key, jsClassParser).mapRight(nList => {
        var lst = F.emptyList<IConfig>(nList.Count);
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var idx = 0; idx < nList.Count; idx++) {
          var n = nList[idx];
          lst.Add(new Config(root, n, $"{(scope == "" ? key : scope + "." + key)}[{idx}]"));
        }
        return (IList<IConfig>) lst;
      });
    }

    Either<ConfigFetchError, A> get<A>(string key, Parser<A> parser, JSONClass current = null) {
      var parts = split(key);

      current = current ?? scopedRoot;
      foreach (var part in parts.dropRight(1)) {
        var either = fetch(current, key, part, jsClassParser);
        if (either.isLeft) return either.mapRight(_ => default(A));
        current = either.rightValue.get;
      }

      return fetch(current, key, parts[parts.Length - 1], parser);
    }

    static string[] split(string key) {
      return key.Split('.');
    }

    Either<ConfigFetchError, IList<A>> getList<A>(
      string key, Parser<A> parser
    ) {
      return get(key, n => F.some(n.AsArray)).flatMapRight(arr => {
        var list = new List<A>(arr.Count);
        for (var idx = 0; idx < arr.Count; idx++) {
          var node = arr[idx];
          var parsed = parser(node);
          if (parsed.isDefined) list.Add(parsed.get);
          else return F.left<ConfigFetchError, IList<A>>(ConfigFetchError.wrongType(
            $"Cannot convert '{key}'[{idx}] to {typeof (A)}: {node}"
          ));
        }
        return F.right<ConfigFetchError, IList<A>>(list);
      });
    }

    Either<ConfigFetchError, A> fetch<A>(
      JSONClass current, string key, string part, Parser<A> parser
    ) {
      if (!current.Contains(part)) 
        return F.left<ConfigFetchError, A>(ConfigFetchError.keyNotFound(
          $"Cannot find part '{part}' from key '{key}' in {current} " +
          $"[scope='{scope}']"
        ));
      var node = current[part];

      return followReference(node).flatMapRight(n => parser(n).fold(
        () => F.left<ConfigFetchError, A>(ConfigFetchError.wrongType(
          $"Cannot convert part '{part}' from key '{key}' to {typeof (A)}. {n.GetType()}" +
          $" Contents: {n}"
        )), F.right<ConfigFetchError, A>
      ));
    }

    Either<ConfigFetchError, JSONNode> followReference(JSONNode current) {
      // references are specified with '#REF=...#'
      if (
        current.Value != null &&
        current.Value.Length >= 6
        && current.Value.Substring(0, 5) == "#REF="
        && current.Value.Substring(current.Value.Length - 1, 1) == "#"
      ) {
        var key = current.Value.Substring(5, current.Value.Length - 6);
        // References are always followed from the root tree.
        return get(key, F.some, root).mapLeft(err =>
          ConfigFetchError.brokenRef($"While following reference {current.Value}: {err}")
        );
      }
      else return F.right<ConfigFetchError, JSONNode>(current);
    }

    public override string ToString() {
      return $"Config(scope: \"{scope}\", data: {scopedRoot})";
    }
  }
}
