using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional {
public static class Option {
  /** 
   * Options are classes on iOS and if we use default(Option<A>) as a 
   * default argument in method parameter list, you'd get a null. To make
   * sure we have a value, use ```Option.ensureValue(ref someOpt);```.
   */
  [Conditional("ENABLE_IL2CPP")]
  public static void ensureValue<A>(ref Option<A> opt) {
#if ENABLE_IL2CPP
    if (opt == null) opt = Option<A>.None;
#endif
  }

  public static IEnumerable<Base> asEnum<Base, Child>(this Option<Child> opt)
  where Child : Base {
    return opt.isDefined ? ((Base) opt.get).Yield() : Enumerable.Empty<Base>();
  }

  public static A getOrNull<A>(this Option<A> opt) where A : class {
    return opt.isDefined ? opt.get : null;
  }

  public static A orNull<A>(this Option<A> opt) where A : class { return opt.getOrNull(); }

  public static Option<A> flatten<A>(this Option<Option<A>> opt) {
    return opt.isDefined ? opt.get : F.none<A>();
  }

  public static Either<A, Option<B>> extract<A, B>(this Option<Either<A, B>> o) {
    foreach (var e in o)
      return e.fold(Either<A, Option<B>>.Left, b => Either<A, Option<B>>.Right(b.some()));
    return Either<A, Option<B>>.Right(F.none<B>());
  }

  public static Option<Base> cast<Child, Base>(this Option<Child> o) where Child : Base
    { return o.isDefined ? F.some((Base) o.get) : F.none<Base>(); }

  /**
   * getOrElse is written as an extension method to make it easier to use in IL2CPP builds
   * where it is a class.
   */

  public static A getOrElse<A>(this Option<A> opt, Fn<A> orElse) {
#if ENABLE_IL2CPP
    if (opt == null) return orElse();
#endif
    return opt.isDefined ? opt.get : orElse();
  }

  public static A getOrElse<A>(this Option<A> opt, A orElse) {
#if ENABLE_IL2CPP
    if (opt == null) return orElse;
#endif
    return opt.isDefined ? opt.get : orElse;
  }
}

public
#if ENABLE_IL2CPP
  class
#else
  struct
#endif
  Option<A>
{
  public static Option<A> None { get; } = new Option<A>();

  readonly A value;
  public readonly bool isSome;

#if ENABLE_IL2CPP
  Option() {}
#endif

  public Option(A value) : this() {
    this.value = value;
    isSome = true;
  }

  public A getOrThrow(Fn<Exception> getEx)
    { return isSome ? value : F.throws<A>(getEx()); }

  public A getOrThrow(string message)
    { return isSome ? value : F.throws<A>(new IllegalStateException(message)); }

  [Obsolete("Use foreach (var item in option) item.doSomething();")]
  public void each(Act<A> action) { if (isSome) action(value); }

  public void onNone(Action action) { if (! isSome) action(); }

  public Option<A> tap(Act<A> action) {
    if (isSome) action(value);
    return this;
  }

  public void voidFold(Action ifEmpty, Act<A> ifNonEmpty) {
    if (isSome) ifNonEmpty(value);
    else ifEmpty();
  }

  public void voidCata(Act<A> ifNonEmpty, Action ifEmpty) { voidFold(ifEmpty, ifNonEmpty); }

  public Option<A> filter(Fn<A, bool> predicate) => 
    isSome ? (predicate(value) ? this : F.none<A>()) : this;

  public bool exists(Fn<A, bool> predicate) {
    return isSome && predicate(value);
  }

  public bool exists(A a) {
    return exists(a, Smooth.Collections.EqComparer<A>.Default);
  }

  public bool exists(A a, IEqualityComparer<A> comparer) {
    return isSome && comparer.Equals(value, a);
  }

  public bool isDefined => isSome;
  public bool isEmpty => ! isSome;

  public A get { get {
    if (isSome) return value;
    throw new IllegalStateException("#get on None!");
  } }

  /* A quick way to get None instance for this options type. */
  public Option<A> none => None;

#region Equality

#if ENABLE_IL2CPP
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

  public OptionEnumerator<A> GetEnumerator() => new OptionEnumerator<A>(this);

  public Option<B> map<B>(Fn<A, B> func) {
    return isDefined ? F.some(func(get)) : F.none<B>();
  }

  public Option<B> flatMap<B>(Fn<A, Option<B>> func) {
    return isDefined ? func(get) : F.none<B>();
  }

  public override string ToString() {
    return isSome ? $"Some({value})" : "None";
  }

  public Either<A, B> toLeft<B>(B right)
    { return isSome ? Either<A, B>.Left(value) : Either<A, B>.Right(right); }
  public Either<A, B> toLeft<B>(Fn<B> right)
    { return isSome ? Either<A, B>.Left(value) : Either<A, B>.Right(right()); }

  public Either<B, A> toRight<B>(B left)
    { return isSome ? Either<B, A>.Right(value) : Either<B, A>.Left(left); }
  public Either<B, A> toRight<B>(Fn<B> left)
    { return isSome ? Either<B, A>.Right(value) : Either<B, A>.Left(left()); }

  public IEnumerable<A> asEnum() {
    return isDefined ? get.Yield() : Enumerable.Empty<A>();
  }

  public Option<A> createOrTap(Fn<A> ifEmpty, Act<A> ifNonEmpty) {
    if (isEmpty) return new Option<A>(ifEmpty());

    ifNonEmpty(get);
    return this;
  }

  [Obsolete("Use opt1 || opt2")]
  public Option<A> orElse(Fn<Option<A>> other) { return this || other(); }

  [Obsolete("Use opt1 || opt2")]
  public Option<A> orElse(Option<A> other) { return this || other; }

  public B fold<B>(Fn<B> ifEmpty, Fn<A, B> ifNonEmpty) {
    return isSome ? ifNonEmpty(get) : ifEmpty();
  }

  public B fold<B>(B ifEmpty, Fn<A, B> ifNonEmpty) {
    return isSome ? ifNonEmpty(get) : ifEmpty;
  }

  public B fold<B>(B ifEmpty, B ifNonEmpty) {
    return isSome ? ifNonEmpty : ifEmpty;
  }

  public B fold<B>(B initial, Fn<A, B, B> ifNonEmpty) => 
    isSome ? ifNonEmpty(get, initial) : initial;

  // Alias for #fold with elements switched up.
  public B cata<B>(Fn<A, B> ifNonEmpty, Fn<B> ifEmpty) {
    return fold(ifEmpty, ifNonEmpty);
  }

  // Alias for #fold with elements switched up.
  public B cata<B>(Fn<A, B> ifNonEmpty, B ifEmpty) {
    return fold(ifEmpty, ifNonEmpty);
  }

  public Option<Tpl<A, B>> zip<B>(Option<B> opt2) => zip(opt2, F.t);

  public Option<C> zip<B, C>(Option<B> opt2, Fn<A, B, C> mapper) => 
    isDefined && opt2.isDefined
    ? F.some(mapper(get, opt2.get))
    : F.none<C>();

  /**
   * If Some() returns None. If None returns b.
   **/
  public Option<B> swap<B>(B b) => isDefined ? F.none<B>() : F.some(b);
  public Option<B> swap<B>(Fn<B> b) => isDefined ? F.none<B>() : F.some(b());

  public static bool operator true(Option<A> opt) { return opt.isDefined; }

  /**
    * Required by |.
    * 
    * http://stackoverflow.com/questions/686424/what-are-true-and-false-operators-in-c#comment43525525_686473
    * The only situation where operator false matters, seems to be if MyClass also overloads 
    * the operator &, in a suitable way. So you can say MyClass conj = GetMyClass1() & GetMyClass2();.
    * Then with operator false you can short-circuit and say 
    * MyClass conj = GetMyClass1() && GetMyClass2();, using && instead of &. That will only 
    * evaluate the second operand if the first one is not "false".  
    **/
  public static bool operator false(Option<A> opt) { return opt.isEmpty; }

  public static Option<A> operator |(Option<A> o1, Option<A> o2) { return o1 ? o1 : o2; }
}

public struct OptionEnumerator<A> {
  public readonly Option<A> option;
  bool read;

  public OptionEnumerator(Option<A> option) : this() { this.option = option; }

  public bool MoveNext() { return option.isDefined && !read; }

  public void Reset() { read = false; }

  public A Current { get {
    read = true;
    return option.get;
  } }
}
}
