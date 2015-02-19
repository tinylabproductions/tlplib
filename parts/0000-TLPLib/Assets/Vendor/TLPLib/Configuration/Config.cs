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
    public static Future<IConfig> apply(string url) {
      return ASync.www(() => new WWW(url)).map(www => {
        var json = JSON.Parse(www.text).AsObject;
        if (json == null) throw new Exception(string.Format(
          "Cannot parse url '{0}' contents as JSON object:\n{1}", 
          url, www.text
        ));
        return (IConfig) new Config(json);
      });
    }

    // Implementation

    private delegate Option<A> Parser<A>(JSONNode node);

    private static readonly Parser<JSONClass> jsClassParser = n => F.opt(n.AsObject);
    private static readonly Parser<string> stringParser = n => F.some(n.Value);
    private static readonly Parser<int> intParser = n => n.Value.parseInt().rightValue;
    private static readonly Parser<float> floatParser = n => n.Value.parseFloat().rightValue;
    private static readonly Parser<bool> boolParser = n => n.Value.parseBool().rightValue;

    private readonly string _scope;
    public override string scope { get { return _scope; } }

    private readonly JSONClass configuration;

    public Config(JSONClass configuration, string scope="") {
      _scope = scope;
      this.configuration = configuration;
    }

    #region either getters

    public override Either<string, string> eitherString(string key) 
    { return get(key, stringParser); }

    public override Either<string, IList<string>> eitherStringList(string key) 
    { return getList(key, stringParser); }

    public override Either<string, int> eitherInt(string key) 
    { return get(key, intParser); }

    public override Either<string, IList<int>> eitherIntList(string key) 
    { return getList(key, intParser); }

    public override Either<string, float> eitherFloat(string key) 
    { return get(key, floatParser); }

    public override Either<string, IList<float>> eitherFloatList(string key) 
    { return getList(key, floatParser); }

    public override Either<string, bool> eitherBool(string key) 
    { return get(key, boolParser); }

    public override Either<string, IList<bool>> eitherBoolList(string key) 
    { return getList(key, boolParser); }

    public override Either<string, IConfig> eitherSubConfig(string key) 
    { return fetchSubConfig(key); }

    public override Either<string, IList<IConfig>> eitherSubConfigList(string key) 
    { return fetchSubConfigList(key); }

    #endregion

    private Either<string, IConfig> fetchSubConfig(string key) {
      return get(key, jsClassParser).mapRight(n => 
        (IConfig) new Config(n, scope == "" ? key : scope + "." + key)
      );
    }

    private Either<string, IList<IConfig>> fetchSubConfigList(string key) {
      return getList(key, jsClassParser).mapRight(nList => {
        var lst = F.emptyList<IConfig>(nList.Count);
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var idx = 0; idx < nList.Count; idx++) {
          var n = nList[idx];
          lst.Add(new Config(n, string.Format(
            "{0}[{1}]", scope == "" ? key : scope + "." + key, idx
          )));
        }
        return (IList<IConfig>) lst;
      });
    }

    private Either<string, A> get<A>(string key, Parser<A> parser) {
      var parts = split(key);

      var current = configuration;
      foreach (var part in parts.dropRight(1)) {
        var either = fetch(current, key, part, jsClassParser);
        if (either.isLeft) return either.mapRight(_ => default(A));
        current = either.rightValue.get;
      }

      return fetch(current, key, parts[parts.Length - 1], parser);
    }

    private static string[] split(string key) {
      return key.Split('.');
    }

    private Either<string, IList<A>> getList<A>(
      string key, Parser<A> parser
    ) {
      return get(key, n => F.some(n.AsArray)).flatMapRight(arr => {
        var list = new List<A>(arr.Count);
        for (var idx = 0; idx < arr.Count; idx++) {
          var node = arr[idx];
          var parsed = parser(node);
          if (parsed.isDefined) list.Add(parsed.get);
          else return F.left<string, IList<A>>(string.Format(
            "Cannot convert '{0}'[{1}] to {2}: {3}",
            key, idx, typeof(A), node
          ));
        }
        return F.right<string, IList<A>>(list);
      });
    }

    private Either<string, A> fetch<A>(
      JSONClass current, string key, string part, Parser<A> parser
    ) {
      if (! current.Contains(part)) return F.left<string, A>(string.Format(
        "Cannot find part '{0}' from key '{1}' in {2}",
        part, key, current
      ));
      var node = current[part];

      return followReference(node).flatMapRight(n => parser(n).fold(
        () => F.left<string, A>(string.Format(
          "Cannot convert part '{0}' from key '{1}' to {2}. {3} Contents: {4}",
          part, key, typeof(A), n.GetType(), n
        )), F.right<string, A>
      ));
    }

    Either<string, JSONNode> followReference(JSONNode current) {
      // references are specified with '#REF=...#'
      if (
        current.Value != null &&
        current.Value.Length >= 6
        && current.Value.Substring(0, 5) == "#REF="
        && current.Value.Substring(current.Value.Length - 1, 1) == "#"
      ) {
        var key = current.Value.Substring(5, current.Value.Length - 6);
        // ReSharper disable once RedundantTypeArgumentsOfMethod
        // Mono compiler bug
        return get<JSONNode>(key, F.some).mapLeft<string>(err =>
          string.Format("While following reference {0}: {1}", current.Value, err)
        );
      }
      else return F.right<string, JSONNode>(current);
    }

    public override string ToString() {
      return string.Format(
        "Config(scope: \"{0}\", data: {1})", scope, configuration
      );
    }
  }
}
