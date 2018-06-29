using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IEnumerableExts {
    /// <summary>
    /// This should really be used only for debugging. It is pretty slow.
    /// </summary>
    [PublicAPI]
    public static string asDebugString(
      this IEnumerable enumerable,
      bool newlines = true, bool fullClasses = false
    ) {
      if (enumerable == null) return "null";
      var sb = new StringBuilder();
      asStringRec(enumerable, sb, newlines, fullClasses, 1);
      return sb.ToString();
    }

    static void asStringRec(
      IEnumerable enumerable, StringBuilder sb,
      bool newlines, bool fullClasses, int indent = 0
    ) {
      var type = enumerable.GetType();
      sb.Append(fullClasses ? type.FullName : type.Name);
      sb.Append('[');

      var first = true;
      foreach (var item in enumerable) {
        if (!first) sb.Append(',');
        if (newlines) {
          sb.Append('\n');
          for (var idx = 0; idx < indent; idx++) sb.Append("  ");
        }
        else if (!first) sb.Append(' ');

        switch (item) {
          case string str:
            sb.Append(str);
            break;
          case IEnumerable enumItem:
            asStringRec(enumItem, sb, newlines, fullClasses, indent + 1);
            break;
          default:
            sb.Append(item);
            break;
        }
        first = false;
      }

      if (newlines) {
        sb.Append('\n');
        for (var idx = 0; idx < indent - 1; idx++) sb.Append("  ");
      }
      sb.Append(']');
    }

    [PublicAPI]
    public static string mkString<A>(
      this IEnumerable<A> e, Act<StringBuilder> appendSeparator,
      string start = null, string end = null
    ) {
      var sb = new StringBuilder();
      if (start != null) sb.Append(start);
      var first = true;
      foreach (var a in e) {
        if (first) first = false;
        else appendSeparator(sb);
        sb.Append(a);
      }
      if (end != null) sb.Append(end);
      return sb.ToString();
    }

    static void throwNullStringBuilderException() {
      // var sb = new StringBuilder();
      // sb.Append("foo");
      // sb.Append('\0');
      // sb.Append("bar");
      // sb.ToString() == "foobar" // -> false
      // sb.ToString() == "foo" // -> true
      throw new Exception(
        "Can't have null char in a separator due to a Mono runtime StringBuilder bug!"
      );
    }

    [PublicAPI]
    public static string mkStringEnum<A>(
      this IEnumerable<A> e, string separator = ", ", string start = "[", string end = "]"
    ) => e.mkString(separator, start, end);

    [PublicAPI]
    public static string mkStringEnumNewLines<A>(
      this IEnumerable<A> e, string separator = ",\n  ", string start = "[\n  ", string end = "\n]"
    ) => e.mkString(separator, start, end);

    [PublicAPI]
    public static string mkString<A>(
      this IEnumerable<A> e, char separator, string start = null, string end = null
    ) {
      if (separator == '\0') throwNullStringBuilderException();
      return e.mkString(sb => sb.Append(separator), start, end);
    }

    [PublicAPI]
    public static string mkString<A>(
      this IEnumerable<A> e, string separator, string start = null, string end = null
    ) {
      if (separator.Contains("\0")) throwNullStringBuilderException();
      return e.mkString(sb => sb.Append(separator), start, end);
    }

    [PublicAPI]
    public static Dictionary<K, A> toDict<A, K>(
      this IEnumerable<KeyValuePair<K, A>> list
    ) => list.toDict(p => p.Key, p => p.Value);

    [PublicAPI]
    public static Dictionary<K, A> toDict<A, K>(
      this IEnumerable<A> list, Fn<A, K> keyGetter
    ) => list.toDict(keyGetter, _ => _);

    // AOT safe version of ToDictionary.
    [PublicAPI]
    public static Dictionary<K, V> toDict<A, K, V>(
      this IEnumerable<A> list, Fn<A, K> keyGetter, Fn<A, V> valueGetter
    ) {
      var dict = new Dictionary<K, V>();
      // ReSharper disable once LoopCanBeConvertedToQuery
      // We're trying to avoid LINQ to avoid iOS AOT related issues.
      foreach (var item in list) {
        var key = keyGetter(item);
        var value = valueGetter(item);
        if (dict.ContainsKey(key)) {
          throw new ArgumentException(
            $"Can't add duplicate key '{key}', current value={dict[key]}, new value={value}"
          );
        }
        dict.Add(key, value);
      }
      return dict;
    }

    [PublicAPI]
    public static IEnumerable<A> Concat<A>(this IEnumerable<A> e, A a) => e.Concat(a.Yield());
    [PublicAPI]
    public static IEnumerable<A> Concat<A>(this IEnumerable<A> e, Option<A> aOpt) =>
      aOpt.isSome ? e.Concat(aOpt.__unsafeGetValue) : e;

    [PublicAPI]
    public static IEnumerable<Base> Concat2<Child, Base>(
      this IEnumerable<Child> e1, IEnumerable<Base> e2
    ) where Child : Base {
      foreach (var _child in e1) yield return _child;
      foreach (var _base in e2) yield return _base;
    }

    [PublicAPI]
    public static IEnumerable<Base> Concat3<Base, Child>(
      this IEnumerable<Base> e1, IEnumerable<Child> e2
    ) where Child : Base {
      foreach (var _base in e1) yield return _base;
      foreach (var _child in e2) yield return _child;
    }

    [PublicAPI]
    public static IEnumerable<A> Yield<A>(this A any) { yield return any; }

    [PublicAPI]
    public static Option<A> find<A>(this IEnumerable<A> enumerable, Fn<A, bool> predicate) {
      foreach (var a in enumerable) if (predicate(a)) return F.some(a);
      return F.none<A>();
    }

    [PublicAPI]
    public static IEnumerable<C> zip<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Fn<A, B, C> f
    ) {
      var aEnum = aEnumerable.GetEnumerator();
      var bEnum = bEnumerable.GetEnumerator();

      while (aEnum.MoveNext() && bEnum.MoveNext())
        yield return f(aEnum.Current, bEnum.Current);

      aEnum.Dispose();
      bEnum.Dispose();
    }

    [PublicAPI]
    public static IEnumerable<C> zipLeft<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Fn<A, B, C> f, Fn<A, int, C> generateMissing
    ) {
      var aEnum = aEnumerable.GetEnumerator();
      var bEnum = bEnumerable.GetEnumerator();

      var idx = -1;
      while (aEnum.MoveNext()) {
        idx++;
        yield return bEnum.MoveNext() ? f(aEnum.Current, bEnum.Current) : generateMissing(aEnum.Current, idx);
      }

      aEnum.Dispose();
      bEnum.Dispose();
    }

    [PublicAPI]
    public static IEnumerable<C> zipRight<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Fn<A, B, C> f, Fn<B, int, C> generateMissing
    ) => bEnumerable.zipLeft(aEnumerable, (b, a) => f(a, b), generateMissing);

    [PublicAPI]
    public static IEnumerable<Tpl<A, B>> zip<A, B>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable
    ) => aEnumerable.zip(bEnumerable, F.t);

    [PublicAPI]
    public static IEnumerable<Tpl<A, int>> zipWithIndex<A>(this IEnumerable<A> enumerable) {
      var idx = 0;
      foreach (var a in enumerable) {
        yield return F.t(a, idx);
        idx += 1;
      }
    }
    
    [PublicAPI]
    public static IEnumerable<A> flatten<A>(this IEnumerable<Option<A>> enumerable) =>
      from aOpt in enumerable
      where aOpt.isSome
      select aOpt.__unsafeGetValue;

    [PublicAPI]
    public static IEnumerable<A> flatten<A>(this IEnumerable<IEnumerable<A>> enumerable) =>
      enumerable.SelectMany(_ => _);

    /// <summary>
    /// Maps enumerable invoking mapper once per distinct A.
    /// </summary>
    [PublicAPI]
    public static IEnumerable<B> mapDistinct<A, B>(
      this IEnumerable<A> enumerable, Fn<A, B> mapper
    ) {
      var cache = new Dictionary<A, B>();
      foreach (var a in enumerable) {
        B b;
        if (!cache.TryGetValue(a, out b)) {
          b = mapper(a);
          cache.Add(a, b);
        }

        yield return b;
      }
    }

    [PublicAPI]
    public static IEnumerable<B> collect<A, B>(
      this IEnumerable<A> enumerable, Fn<A, Option<B>> collector
    ) {
      foreach (var a in enumerable) {
        var bOpt = collector(a);
        if (bOpt.isSome) yield return bOpt.__unsafeGetValue;
      }
    }

    [PublicAPI]
    public static IEnumerable<B> collect<A, B>(
      this IEnumerable<A> enumerable, Fn<A, int, Option<B>> collector
    ) {
      var idx = 0;
      foreach (var a in enumerable) {
        var bOpt = collector(a, idx);
        if (bOpt.isSome) yield return bOpt.__unsafeGetValue;
        idx++;
      }
    }

    [PublicAPI]
    public static Option<B> collectFirst<A, B>(
      this IEnumerable<A> enumerable, Fn<A, Option<B>> collector
    ) {
      foreach (var a in enumerable) {
        var bOpt = collector(a);
        if (bOpt.isSome) return bOpt;
      }
      return F.none<B>();
    }

    [PublicAPI]
    public static HashSet<A> toHashSet<A>(this IEnumerable<A> enumerable) =>
      new HashSet<A>(enumerable);

    /// <summary>Partitions enumerable into two lists using a predicate.</summary>
    [PublicAPI]
    public static Partitioned<A> partition<A>(this IEnumerable<A> enumerable, Fn<A, bool> predicate) {
      var trues = ImmutableList.CreateBuilder<A>();
      var falses = ImmutableList.CreateBuilder<A>();
      foreach (var a in enumerable) (predicate(a) ? trues : falses).Add(a);
      return Partitioned.a(trues.ToImmutable(), falses.ToImmutable());
    }

    [PublicAPI]
    public static Tpl<ImmutableList<A>, ImmutableList<B>> partitionCollect<A, B>(
      this IEnumerable<A> enumerable, Fn<A, Option<B>> collector
    ) {
      var nones = ImmutableList.CreateBuilder<A>();
      var somes = ImmutableList.CreateBuilder<B>();
      foreach (var a in enumerable) {
        var bOpt = collector(a);
        if (bOpt.isSome)
          somes.Add(bOpt.__unsafeGetValue);
        else
          nones.Add(a);
      }
      return F.t(nones.ToImmutable(), somes.ToImmutable());
    }

    [PublicAPI]
    public static IOrderedEnumerable<A> OrderBySafe<A, B>(
      this IEnumerable<A> source, Func<A, B> keySelector
    ) where B : IComparable<B> => source.OrderBy(keySelector);

    /// <summary>Take <see cref="count"/> random unique elements from a finite enumerable.</summary>
    [PublicAPI]
    public static ImmutableList<A> takeRandomly<A>(
      this IEnumerable<A> enumerable, int count, ref Rng rng
    ) {
      var r = rng;
      // Need to force the evaluation to update rng state.
      var result = enumerable.OrderBySafe(_ => r.nextInt(out r)).Take(count).ToImmutableList();
      rng = r;
      return result;
    }

    [PublicAPI]
    public static bool isEmpty<A>(this IEnumerable<A> enumerable) => !enumerable.Any();
    [PublicAPI]
    public static bool nonEmpty<A>(this IEnumerable<A> enumerable) => enumerable.Any();

    [PublicAPI]
    public static IEnumerable<A> Except<A>(
      this IEnumerable<A> enumerable, A except, IEqualityComparer<A> cmp = null
    ) {
      cmp = cmp ?? EqComparer<A>.Default;
      return enumerable.Where(a => !cmp.Equals(a, except));
    }

    [PublicAPI]
    public static Option<A> headOption<A>(this IEnumerable<A> enumerable) {
      foreach (var a in enumerable)
        return F.some(a);
      return F.none<A>();
    }

    /// <summary>
    /// Aggregate with index passed into reducer.
    /// </summary>
    [PublicAPI]
    public static B Aggregate<A, B>(
      this IEnumerable<A> enumerable, B initial, Fn<A, B, int, B> reducer
    ) {
      var reduced = initial;
      var idx = 0;
      foreach (var a in enumerable) {
        reduced = reducer(a, reduced, idx);
        idx++;
      }
      return reduced;
    }
  }

  public struct Partitioned<A> : IEquatable<Partitioned<A>> {
    public readonly ImmutableList<A> trues, falses;

    public Partitioned(ImmutableList<A> trues, ImmutableList<A> falses) {
      this.trues = trues;
      this.falses = falses;
    }

    #region Equality

    public bool Equals(Partitioned<A> other) {
      return trues.SequenceEqual(other.trues) && falses.SequenceEqual(other.falses);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Partitioned<A> && Equals((Partitioned<A>) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return ((trues != null ? trues.GetHashCode() : 0) * 397) ^ (falses != null ? falses.GetHashCode() : 0);
      }
    }

    public static bool operator ==(Partitioned<A> left, Partitioned<A> right) { return left.Equals(right); }
    public static bool operator !=(Partitioned<A> left, Partitioned<A> right) { return !left.Equals(right); }

    #endregion

    public override string ToString() =>
      $"{nameof(Partitioned)}[" +
      $"{nameof(trues)}: {trues.asDebugString()}, " +
      $"{nameof(falses)}: {falses.asDebugString()}" +
      $"]";
  }

  public static class Partitioned {
    public static Partitioned<A> a<A>(ImmutableList<A> trues, ImmutableList<A> falses) =>
      new Partitioned<A>(trues, falses);
  }
}