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
 **/
public static class Option {
  public static IEnumerable<A> asEnum<A, B>(this Option<B> opt)
  where B : class, A  {
    return opt.isDefined 
      ? (IEnumerable<A>) opt.get.Yield() 
      : Enumerable.Empty<A>();
  }

  public static A orNull<A>(Option<A> opt) where A : class {
    return opt.fold(() => null, _ => _);
  }
}

public struct Option<A> {
  public static Option<A> None { get { return new Option<A>(); } }

  private readonly A value;
  public readonly bool isSome;

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
  public Option<A> none { get { return F.none<A>(); } }

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

  public override string ToString() {
    return isSome ? "Some(" + value + ")" : "None";
  }

  public IEnumerable<A> asEnum { get { return isSome ? value.Yield() : Enumerable.Empty<A>(); } }

  public Option<A> createOrTap(Fn<A> ifEmpty, Act<A> ifNonEmpty) {
    if (isEmpty) return new Option<A>(ifEmpty());

    ifNonEmpty(value);
    return this;
  }

  public Option<A> orElse(Fn<Option<A>> other) { return isSome ? this : other(); }
  public Option<A> orElse(Option<A> other) { return isSome ? this : other; }

  // Alias for #fold with elements switched up.
  public B cata<B>(Fn<A, B> ifNonEmpty, Fn<B> ifEmpty) { return fold(ifEmpty, ifNonEmpty); }

  // Alias for #fold with elements switched up.
  public B cata<B>(Fn<A, B> ifNonEmpty, B ifEmpty) { return fold(ifEmpty, ifNonEmpty); }

  public void voidFold(Act ifEmpty, Act<A> ifNonEmpty) {
    if (isSome) ifNonEmpty(value);
    else ifEmpty();
  }

  public Option<B> map<B>(Fn<A, B> func) {
    return isSome ? F.some(func(value)) : F.none<B>();
  }

  public Option<B> flatMap<B>(Fn<A, Option<B>> func) {
    return isSome ? func(value) : F.none<B>();
  }

  public Option<Tpl<A, B>> zip<B>(Option<B> opt2) {
    return isSome && opt2.isDefined
      ? F.some(F.t(value, opt2.get))
      : F.none<Tpl<A, B>>();
  }

  public B fold<B>(Fn<B> ifEmpty, Fn<A, B> ifNonEmpty) {
    return isSome ? ifNonEmpty(value) : ifEmpty();
  }

  public B fold<B>(B ifEmpty, Fn<A, B> ifNonEmpty) {
    return isSome ? ifNonEmpty(value) : ifEmpty;
  }

  public Either<B, A> toEither<B>(Fn<B> ifEmpty) {
    return isSome ? new Either<B, A>(value) : new Either<B, A>(ifEmpty());
  }

  public Either<B, A> toEither<B>(B ifEmpty) {
    return isSome ? new Either<B, A>(value) : new Either<B, A>(ifEmpty);
  }

  public A getOrElse(Fn<A> orElse) { return isSome ? value : orElse(); }

  public A getOrElse(A orElse) { return isSome ? value : orElse; }

  /**
   * If this option is:
   * * None - returns Some(ifEmpty())
   * * Some - applies ifSome and returns None.
   */
  public Option<B> swap<B>(Fn<B> ifEmpty, Act<A> ifSome=null) {
    return fold(
      () => F.some(ifEmpty()),
      v => {
        if (ifSome != null) ifSome(v);
        return F.none<B>();
      }
    );
  }

  public Option<B> swap<B>(B ifEmpty, Act<A> ifSome=null) {
    return fold(
      () => F.some(ifEmpty),
      v => {
        if (ifSome != null) ifSome(v);
        return F.none<B>();
      }
    );
  }
}
}
