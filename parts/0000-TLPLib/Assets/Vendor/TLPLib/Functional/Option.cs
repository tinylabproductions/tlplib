using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional {
/** 
 * Hack to glue-on contravariant type parameter requirements
 * 
 * http://stackoverflow.com/questions/1188354/can-i-specify-a-supertype-relation-in-c-sharp-generic-constraints
 * 
 * Beware that this causes boxing for value types.
 * 
 * Also Mono compiler seems to be a lot happier with generic extension 
 * methods, than instance ones. 
 **/
public static class Option {
  public static IEnumerable<A> asEnum<A, B>(this Option<B> opt)
  where B : A {
    return opt.isDefined 
      ? (IEnumerable<A>) opt.get.Yield() 
      : Enumerable.Empty<A>();
  }

  public static IEnumerable<A> asEnum<A>(this Option<A> opt) {
    return opt.isDefined ? opt.get.Yield() : Enumerable.Empty<A>();
  }

  public static A getOrElse<A>(this Option<A> opt, Fn<A> orElse) {
    return opt.isDefined ? opt.get : orElse();
  }

  public static A getOrElse<A>(this Option<A> opt, A orElse) {
    return opt.isDefined ? opt.get : orElse;
  }

  public static Option<A> createOrTap<A>(
    this Option<A> opt, Fn<A> ifEmpty, Act<A> ifNonEmpty
  ) {
    if (opt.isEmpty) return new Option<A>(ifEmpty());

    ifNonEmpty(opt.get);
    return opt;
  }

  public static Option<A> orElse<A>(this Option<A> opt, Fn<Option<A>> other) {
    return opt.isDefined ? opt : other();
  }

  public static Option<A> orElse<A>(this Option<A> opt, Option<A> other) {
    return opt.isDefined ? opt : other;
  }

  public static A orNull<A>(this Option<A> opt) where A : class {
    return opt.fold(() => null, _ => _);
  }

  public static B fold<A, B>(this Option<A> opt, Fn<B> ifEmpty, Fn<A, B> ifNonEmpty) {
    return opt.isSome ? ifNonEmpty(opt.get) : ifEmpty();
  }

  public static B fold<A, B>(this Option<A> opt, B ifEmpty, Fn<A, B> ifNonEmpty) {
    return opt.isSome ? ifNonEmpty(opt.get) : ifEmpty;
  }

  // Alias for #fold with elements switched up.
  public static B cata<A, B>(this Option<A> opt, Fn<A, B> ifNonEmpty, Fn<B> ifEmpty) {
    return opt.fold(ifEmpty, ifNonEmpty);
  }

  // Alias for #fold with elements switched up.
  public static B cata<A, B>(this Option<A> opt, Fn<A, B> ifNonEmpty, B ifEmpty) {
    return opt.fold(ifEmpty, ifNonEmpty);
  }

  public static void voidFold<A>(this Option<A> opt, Action ifEmpty, Act<A> ifNonEmpty) {
    if (opt.isSome) ifNonEmpty(opt.get);
    else ifEmpty();
  }
  // Downcast an option.
  public static Option<B> to<A, B>(this Option<A> opt) where A : B {
    return opt.map(_ => (B) _);
  }

  public static Option<B> map<A, B>(this Option<A> opt, Fn<A, B> func) {
    return opt.isDefined ? F.some(func(opt.get)) : F.none<B>();
  }

  public static Option<B> flatMap<A, B>(
    this Option<A> opt, Fn<A, Option<B>> func
  ) {
    return opt.isDefined ? func(opt.get) : F.none<B>();
  }

  public static Option<A> flatten<A>(this Option<Option<A>> opt) {
    return opt.isDefined ? opt.get : F.none<A>();
  }

  public static Option<Tpl<A, B>> zip<A, B>(
    this Option<A> opt1, Option<B> opt2
  ) {
    return opt1.isDefined && opt2.isDefined
      ? F.some(F.t(opt1.get, opt2.get))
      : F.none<Tpl<A, B>>();
  }
}

public 
#if UNITY_IOS
  class
#else
  struct 
#endif
	Option<A> 
{
  public static Option<A> None { get { return new Option<A>(); } }

  private readonly A value;
  public readonly bool isSome;

#if UNITY_IOS
  public Option() {}
#endif

  public Option(A value) : this() {
    this.value = value;
    isSome = true;
  }

  public A getOrThrow(Fn<Exception> getEx) 
    { return isSome ? value : F.throws<A>(getEx()); }

  public void each(Act<A> action) { if (isSome) action(value); }

  public void onNone(Act action) { if (! isSome) action(); }

  public Option<A> tap(Act<A> action) {
    if (isSome) action(value);
    return this;
  }

  public void voidFold(Action ifEmpty, Act<A> ifNonEmpty) {
    if (isSome) ifNonEmpty(value);
    else ifEmpty();
  }

  public Option<A> filter(Fn<A, bool> predicate) {
    return (isSome ? (predicate(value) ? this : F.none<A>()) : this);
  }

  public bool exists(Fn<A, bool> predicate) {
    return isSome && predicate(value);
  }

  public bool exists(A a) {
    return exists(a, Smooth.Collections.EqComparer<A>.Default);
  }

  public bool exists(A a, IEqualityComparer<A> comparer) {
    return isSome && comparer.Equals(value, a);
  }

  public bool isDefined { get { return isSome; } }
  public bool isEmpty { get { return ! isSome; } }

  public A get { get {
    if (isSome) return value;
    throw new IllegalStateException("#get on None!");
  } }

  /* A quick way to get None instance for this options type. */
  public Option<A> none => F.none<A>();

  #region Equality

#if UNITY_IOS
  protected bool Equals(Option<A> other) {
    return EqualityComparer<A>.Default.Equals(value, other.value) && isSome == other.isSome;
  }

  public override bool Equals(object obj) {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != this.GetType()) return false;
    return Equals((Option<A>) obj);
  }

  public override int GetHashCode() {
    unchecked {
      return (EqualityComparer<A>.Default.GetHashCode(value) * 397) ^ isSome.GetHashCode();
    }
  }

  sealed class ValueIsSomeEqualityComparer : IEqualityComparer<Option<A>> {
    public bool Equals(Option<A> x, Option<A> y) {
      if (ReferenceEquals(x, y)) return true;
      if (ReferenceEquals(x, null)) return false;
      if (ReferenceEquals(y, null)) return false;
      if (x.GetType() != y.GetType()) return false;
      return EqualityComparer<A>.Default.Equals(x.value, y.value) && x.isSome == y.isSome;
    }

    public int GetHashCode(Option<A> obj) {
      unchecked {
        return (EqualityComparer<A>.Default.GetHashCode(obj.value) * 397) ^ obj.isSome.GetHashCode();
      }
    }
  }

  static readonly IEqualityComparer<Option<A>> valueIsSomeComparerInstance = new ValueIsSomeEqualityComparer();

  public static IEqualityComparer<Option<A>> valueIsSomeComparer
  {
    get { return valueIsSomeComparerInstance; }
  }

#else
  public override bool Equals(object o) {
    return o is Option<A> && Equals((Option<A>)o);
  }

  public bool Equals(Option<A> other) {
    return isSome ? other.exists(value) : other.isEmpty;
  }

  public override int GetHashCode() {
    return Smooth.Collections.EqComparer<A>.Default.GetHashCode(value);
  }

  public static bool operator == (Option<A> lhs, Option<A> rhs) {
    return lhs.Equals(rhs);
  }

  public static bool operator != (Option<A> lhs, Option<A> rhs) {
    return !lhs.Equals(rhs);
  }
#endif

  #endregion

  public override string ToString() {
    return isSome ? "Some(" + value + ")" : "None";
  }
}
}
