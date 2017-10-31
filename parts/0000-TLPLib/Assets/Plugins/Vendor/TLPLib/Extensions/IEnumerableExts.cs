﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IEnumerableExts {
    /* This should really be used only for debugging. It is pretty slow. */

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

        var str = item as string; // String is IEnumerable as well
        if (str != null) sb.Append(str);
        else {
          var enumItem = item as IEnumerable;
          if (enumItem != null) asStringRec(enumItem, sb, newlines, fullClasses, indent + 1);
          else sb.Append(item);
        }
        first = false;
      }

      if (newlines) {
        sb.Append('\n');
        for (var idx = 0; idx < indent - 1; idx++) sb.Append("  ");
      }
      sb.Append(']');
    }

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

    public static string mkStringEnum<A>(
      this IEnumerable<A> e, string separator = ", ", string start = "[", string end = "]"
    ) => e.mkString(separator, start, end);

    public static string mkStringEnumNewLines<A>(
      this IEnumerable<A> e, string separator = ",\n  ", string start = "[\n  ", string end = "\n]"
    ) => e.mkString(separator, start, end);

    public static string mkString<A>(
      this IEnumerable<A> e, char separator, string start = null, string end = null
    ) {
      if (separator == '\0') throwNullStringBuilderException();
      return e.mkString(sb => sb.Append(separator), start, end);
    }

    public static string mkString<A>(
      this IEnumerable<A> e, string separator, string start = null, string end = null
    ) {
      if (separator.Contains("\0")) throwNullStringBuilderException();
      return e.mkString(sb => sb.Append(separator), start, end);
    }

    public static Dictionary<K, A> toDict<A, K>(
      this IEnumerable<KeyValuePair<K, A>> list
    ) => list.toDict(p => p.Key, p => p.Value);

    public static Dictionary<K, A> toDict<A, K>(
      this IEnumerable<A> list, Fn<A, K> keyGetter
    ) => list.toDict(keyGetter, _ => _);

    // AOT safe version of ToDictionary.
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

    public static IEnumerable<A> Concat<A>(this IEnumerable<A> e, A a) => e.Concat(a.Yield());
    public static IEnumerable<A> Concat<A>(this IEnumerable<A> e, Option<A> aOpt) =>
      aOpt.isSome ? e.Concat(aOpt.__unsafeGetValue) : e;

    public static IEnumerable<Base> Concat2<Child, Base>(
      this IEnumerable<Child> e1, IEnumerable<Base> e2
    ) where Child : Base {
      foreach (var _child in e1) yield return _child;
      foreach (var _base in e2) yield return _base;
    }

    public static IEnumerable<Base> Concat3<Base, Child>(
      this IEnumerable<Base> e1, IEnumerable<Child> e2
    ) where Child : Base {
      foreach (var _base in e1) yield return _base;
      foreach (var _child in e2) yield return _child;
    }

    public static IEnumerable<A> Yield<A>(this A any) { yield return any; }

    [Obsolete("Use foreach instead.")] public static void each<A>(this IEnumerable<A> enumerable, Act<A> f) {
      foreach (var el in enumerable) f(el);
    }

    public static Option<A> find<A>(this IEnumerable<A> enumerable, Fn<A, bool> predicate) {
      foreach (var a in enumerable) if (predicate(a)) return F.some(a);
      return F.none<A>();
    }

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

    public static IEnumerable<C> zipRight<A, B, C>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable, Fn<A, B, C> f, Fn<B, int, C> generateMissing
    ) => bEnumerable.zipLeft(aEnumerable, (b, a) => f(a, b), generateMissing);

    public static IEnumerable<Tpl<A, B>> zip<A, B>(
      this IEnumerable<A> aEnumerable, IEnumerable<B> bEnumerable
    ) => aEnumerable.zip(bEnumerable, F.t);

    public static IEnumerable<Tpl<A, int>> zipWithIndex<A>(this IEnumerable<A> enumerable) {
      var idx = 0;
      foreach (var a in enumerable) {
        yield return F.t(a, idx);
        idx += 1;
      }
    }

    public static IEnumerable<A> flatten<A>(this IEnumerable<Option<A>> enumerable) {
      return enumerable.SelectMany(_ => _.asEnum());
    }

    public static IEnumerable<A> flatten<A>(this IEnumerable<IEnumerable<A>> enumerable) {
      return enumerable.SelectMany(_ => _);
    }

    public static IEnumerable<B> collect<A, B>(
      this IEnumerable<A> enumerable, Fn<A, Option<B>> collector
    ) {
      foreach (var a in enumerable) {
        var bOpt = collector(a);
        if (bOpt.isSome) yield return bOpt.__unsafeGetValue;
      }
    }

    public static Option<B> collectFirst<A, B>(
      this IEnumerable<A> enumerable, Fn<A, Option<B>> collector
    ) {
      foreach (var a in enumerable) {
        var bOpt = collector(a);
        if (bOpt.isSome) return bOpt;
      }
      return F.none<B>();
    }

    public static HashSet<A> toHashSet<A>(this IEnumerable<A> enumerable) { return new HashSet<A>(enumerable); }

    /** Partitions enumerable into two lists using a predicate: (all false elements, all true elements) **/

    public static Partitioned<A> partition<A>(this IEnumerable<A> enumerable, Fn<A, bool> predicate) {
      var trues = new List<A>();
      var falses = new List<A>();
      foreach (var a in enumerable) (predicate(a) ? trues : falses).Add(a);
      return Partitioned.a(trues, falses);
    }

    public static IOrderedEnumerable<A> OrderBySafe<A, B>(
      this IEnumerable<A> source, Func<A, B> keySelector
    ) where B : IComparable<B> => source.OrderBy(keySelector);

    /// <summary>Take <see cref="count"/> random unique elements from a finite enumerable.</summary>
    public static ImmutableList<A> takeRandomly<A>(
      this IEnumerable<A> enumerable, int count, ref Rng rng
    ) {
      var r = rng;
      // Need to force the evaluation to update rng state.
      var result = enumerable.OrderBySafe(_ => r.nextInt(out r)).Take(count).ToImmutableList();
      rng = r;
      return result;
    }
  }

  public struct Partitioned<A> : IEquatable<Partitioned<A>> {
    public readonly List<A> trues, falses;

    public Partitioned(List<A> trues, List<A> falses) {
      this.trues = trues;
      this.falses = falses;
    }

    public bool Equals(Partitioned<A> other) {
      throw new InvalidOperationException(
        "Can't compare two partitioned datasets, because their lists are mutable!"
      );
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Partitioned<A> && Equals((Partitioned<A>)obj);
    }

    public override int GetHashCode() { return base.GetHashCode(); }

    public override string ToString() {
      return $"{nameof(Partitioned)}[" +
             $"{nameof(trues)}: {trues.asDebugString()}, " +
             $"{nameof(falses)}: {falses.asDebugString()}" +
             $"]";
    }
  }

  public static class Partitioned {
    public static Partitioned<A> a<A>(List<A> trues, List<A> falses) { return new Partitioned<A>(trues, falses); }
  }
}