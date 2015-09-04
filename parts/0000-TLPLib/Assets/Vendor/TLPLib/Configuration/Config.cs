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
      return new WWW(url).wwwFuture().map(www => {
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
    private static readonly Parser<DateTime> dateTimeParser = n => n.Value.parseDateTime().rightValue;

    private readonly string _scope;
    public override string scope { get { return _scope; } }

    readonly JSONClass root, scopedRoot;

    public Config(JSONClass root, JSONClass scopedRoot=null, string scope="") {
      _scope = scope;
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

    private Either<ConfigFetchError, IConfig> fetchSubConfig(string key) {
      return get(key, jsClassParser).mapRight(n => 
        (IConfig) new Config(root, n, scope == "" ? key : scope + "." + key)
      );
    }

    private Either<ConfigFetchError, IList<IConfig>> fetchSubConfigList(string key) {
      return getList(key, jsClassParser).mapRight(nList => {
        var lst = F.emptyList<IConfig>(nList.Count);
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var idx = 0; idx < nList.Count; idx++) {
          var n = nList[idx];
          lst.Add(new Config(root, n, string.Format(
            "{0}[{1}]", scope == "" ? key : scope + "." + key, idx
          )));
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

    private static string[] split(string key) {
      return key.Split('.');
    }

    private Either<ConfigFetchError, IList<A>> getList<A>(
      string key, Parser<A> parser
    ) {
      return get(key, n => F.some(n.AsArray)).flatMapRight(arr => {
        var list = new List<A>(arr.Count);
        for (var idx = 0; idx < arr.Count; idx++) {
          var node = arr[idx];
          var parsed = parser(node);
          if (parsed.isDefined) list.Add(parsed.get);
          else return F.left<ConfigFetchError, IList<A>>(ConfigFetchError.wrongType(string.Format(
            "Cannot convert '{0}'[{1}] to {2}: {3}",
            key, idx, typeof(A), node
          )));
        }
        return F.right<ConfigFetchError, IList<A>>(list);
      });
    }

    private Either<ConfigFetchError, A> fetch<A>(
      JSONClass current, string key, string part, Parser<A> parser
    ) {
      if (!current.Contains(part)) 
        return F.left<ConfigFetchError, A>(ConfigFetchError.keyNotFound(string.Format(
          "Cannot find part '{0}' from key '{1}' in {2} [scope='{3}']",
          part, key, current, scope
        )));
      var node = current[part];

      return followReference(node).flatMapRight(n => parser(n).fold(
        () => F.left<ConfigFetchError, A>(ConfigFetchError.wrongType(string.Format(
          "Cannot convert part '{0}' from key '{1}' to {2}. {3} Contents: {4}",
          part, key, typeof(A), n.GetType(), n
        ))), F.right<ConfigFetchError, A>
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
        // ReSharper disable once RedundantTypeArgumentsOfMethod
        // Mono compiler bug
        // References are always followed from the root tree.
        return get<JSONNode>(key, F.some, root).mapLeft(err =>
          ConfigFetchError.brokenRef(
            string.Format("While following reference {0}: {1}", current.Value, err)
          )
        );
      }
      else return F.right<ConfigFetchError, JSONNode>(current);
    }

    public override string ToString() {
      return string.Format(
        "Config(scope: \"{0}\", data: {1})", scope, scopedRoot
      );
    }
  }
}
