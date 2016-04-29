using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Configuration {
  /* IConfig that allows string, int, float, bool, list (including config) 
   * key overrides based on variables. */
  public class VariableConfig : ConfigBase {
    public readonly IConfig underlying;
    readonly Dictionary<string, string> variables;
    readonly IList<IList<string>> combinations;

    /* [param variables] variable name -> value pairs
     * 
     * [param combinations] [[varname, varname, ...], ...] combinations that 
     * need to be checked when fetching a key. So if you have 
     * [["a", "b"], ["b"]] specified, following keys will be checked if you
     * try to fetch "some.key":
     * - "some.a.b.key"
     * - "some.b.key"
     * - "some.key"
     */
    public VariableConfig(
      IConfig underlying, Dictionary<string, string> variables, 
      IList<IList<string>> combinations
    ) {
      this.underlying = underlying;
      this.variables = variables;
      this.combinations = combinations;

      var missingVariables = combinations.SelectMany(combination =>
        combination.Where(variable => ! variables.ContainsKey(variable))
      ).ToArray();
      if (missingVariables.Length > 0) throw new ArgumentException(
        "Variables '" + missingVariables.asString(false) + "' are missing. " +
        "Defined variables: " + variables.asString(false)
      );
    }

    public override string ToString() {
      return $"VariableConfig(variables: {variables.asString(false)}, combinations: {combinations.asString(false)}, underlying: {underlying})";
    }

    public override string scope { get { return underlying.scope; } }

    public override Either<ConfigFetchError, object> eitherObject(string key) 
    { return injected(key, k => underlying.eitherObject(k)); }

    public override Either<ConfigFetchError, string> eitherString(string key) 
    { return injected(key, k => underlying.eitherString(k)); }

    public override Either<ConfigFetchError, IList<string>> eitherStringList(string key) 
    { return injected(key, k => underlying.eitherStringList(k)); }

    public override Either<ConfigFetchError, int> eitherInt(string key) 
    { return injected(key, k => underlying.eitherInt(k)); }

    public override Either<ConfigFetchError, IList<int>> eitherIntList(string key) 
    { return injected(key, k => underlying.eitherIntList(k)); }

    public override Either<ConfigFetchError, long> eitherLong(string key)
    { return injected(key, k => underlying.eitherLong(k)); }

    public override Either<ConfigFetchError, IList<long>> eitherLongList(string key)
    { return injected(key, k => underlying.eitherLongList(k)); }

    public override Either<ConfigFetchError, float> eitherFloat(string key) 
    { return injected(key, k => underlying.eitherFloat(k)); }

    public override Either<ConfigFetchError, IList<float>> eitherFloatList(string key) 
    { return injected(key, k => underlying.eitherFloatList(k)); }

    public override Either<ConfigFetchError, bool> eitherBool(string key)
    { return injected(key, k => underlying.eitherBool(k)); }

    public override Either<ConfigFetchError, IList<bool>> eitherBoolList(string key) 
    { return injected(key, k => underlying.eitherBoolList(k)); }

    public override Either<ConfigFetchError, DateTime> eitherDateTime(string key) 
    { return injected(key, k => underlying.eitherDateTime(k)); }

    public override Either<ConfigFetchError, IList<DateTime>> eitherDateTimeList(string key) 
    { return injected(key, k => underlying.eitherDateTimeList(k)); }

    public override Either<ConfigFetchError, IConfig> eitherSubConfig(string key) {
      // ReSharper disable once RedundantTypeArgumentsOfMethod
      // Mono compiler bug.
      return underlying.eitherSubConfig(key).mapRight<IConfig>(wrapConfig);
    }

    public override Either<ConfigFetchError, IList<IConfig>> eitherSubConfigList(
      string key
    ) {
      return injected(key, k => underlying.eitherSubConfigList(k).mapRight(lst =>
        // ReSharper disable once RedundantTypeArgumentsOfMethod
        // Mono compiler bug.
        (IList<IConfig>) lst.Select<IConfig, IConfig>(wrapConfig).ToList()
      ));
    }

    private IConfig wrapConfig(IConfig c) 
    { return new VariableConfig(c, variables, combinations); }

    /* Checks all the keys, returns the last value if none of them 
     * return a value. */
    private Either<ConfigFetchError, A> injected<A>(
      string key, Fn<string, Either<ConfigFetchError, A>> getter
    ) {
      var keys = injectToKey(key, combinations, variables).GetEnumerator();

      keys.MoveNext();
      var current = getter(keys.Current);
      while (keys.MoveNext()) {
        if (
          current.isRight || 
          (current.isLeft && current.leftValue.get.kind != ConfigFetchError.Kind.KEY_NOT_FOUND)
        ) return current;
        current = getter(keys.Current);
      }

      return current;
    }

    /* Injects combinations into key. */
    public static IEnumerable<string> injectToKey(
      string key, IList<IList<string>> combinations, 
      Dictionary<string, string> variables
    ) {
      var injected = injectToKey(
        key, 
        combinations.Select(combination =>
          string.Join(".", combination.Select(var => variables[var]).ToArray())
        )
      );
      foreach (var injectedKey in injected) yield return injectedKey;
      // Try just a key as a last resort.
      yield return key;
    }

    /* Injects each of the parts before last dot into key. */
    public static IEnumerable<string> injectToKey(string key, IEnumerable<string> parts) {
      var lastIndex = key.LastIndexOf('.');
      if (lastIndex == -1)
        foreach (var part in parts) yield return part + "." + key;
      else
        foreach (var part in parts)
          yield return 
            key.Substring(0, lastIndex) + "." + part + 
            key.Substring(lastIndex);
    }
  }
}
