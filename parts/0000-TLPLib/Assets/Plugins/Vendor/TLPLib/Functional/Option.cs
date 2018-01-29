using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
      if ((object) opt == null) opt = Option<A>.None;
#endif
    }

    /// <summary>
    /// Example usage:
    /// 
    /// <code><![CDATA[
    /// Option<NonEmpty<ImmutableList<DraggablePart>>> __parts = 
    ///   Option<NonEmpty<ImmutableList<DraggablePart>>>.None;
    /// public NonEmpty<ImmutableList<DraggablePart>> parts =>
    ///   Option.getOrUpdate(ref __parts, () => _parts.ToImmutableList().toNonEmpty().get);
    /// ]]></code>
    /// </summary>
    public static A getOrUpdate<A>(ref Option<A> opt, Fn<A> create) {
      if (opt.isNone) opt = new Option<A>(create());
      return opt.__unsafeGetValue;
    }

    public static IEnumerable<Base> asEnum<Base, Child>(this Option<Child> opt) where Child : Base => 
      opt.isSome ? ((Base) opt.get).Yield() : Enumerable.Empty<Base>();

    public static A getOrNull<A>(this Option<A> opt) where A : class => 
      opt.isSome ? opt.get : null;

    public static A orNull<A>(this Option<A> opt) where A : class => 
      opt.getOrNull();

    public static Option<A> flatten<A>(this Option<Option<A>> opt) => 
      opt.isSome ? opt.__unsafeGetValue : F.none<A>();

    public static Option<Base> cast<Child, Base>(this Option<Child> o) where Child : Base
      { return o.isSome ? F.some((Base) o.get) : F.none<Base>(); }

    /**
     * getOrElse is written as an extension method to make it easier to use in IL2CPP builds
     * where it is a class.
     */
    public static A getOrElse<A>(this Option<A> opt, Fn<A> orElse) {
#if ENABLE_IL2CPP
      if (opt == null) return orElse();
#endif
      return opt.isSome ? opt.get : orElse();
    }

    public static A getOrElse<A>(this Option<A> opt, A orElse) {
#if ENABLE_IL2CPP
      if (opt == null) return orElse;
#endif
      return opt.isSome ? opt.get : orElse;
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

    public readonly A __unsafeGetValue;
    public readonly bool isSome;

#if ENABLE_IL2CPP
    Option() {}
#endif

    public Option(A value) : this() {
      __unsafeGetValue = value;
      isSome = true;
    }

    public A getOrThrow(Fn<Exception> getEx) => 
      isSome ? __unsafeGetValue : F.throws<A>(getEx());

    public A getOrThrow(string message) => 
      isSome ? __unsafeGetValue : F.throws<A>(new IllegalStateException(message));

    public void onNone(Action action) { if (! isSome) action(); }

    public Option<A> tap(Act<A> action) {
      if (isSome) action(__unsafeGetValue);
      return this;
    }

    public void voidFold(Action ifEmpty, Act<A> ifNonEmpty) {
      if (isSome) ifNonEmpty(__unsafeGetValue);
      else ifEmpty();
    }

    public void voidCata(Act<A> ifNonEmpty, Action ifEmpty) => 
      voidFold(ifEmpty, ifNonEmpty);

    public Option<A> filter(bool keepValue) =>
      keepValue ? this : F.none<A>();

    public Option<A> filter(Fn<A, bool> predicate) => 
      isSome && predicate(__unsafeGetValue) ? this : F.none<A>();

    public bool exists(Fn<A, bool> predicate) => 
      isSome && predicate(__unsafeGetValue);

    public bool exists(A a) => 
      exists(a, Smooth.Collections.EqComparer<A>.Default);

    public bool exists(A a, IEqualityComparer<A> comparer) => 
      isSome && comparer.Equals(__unsafeGetValue, a);

    public bool isNone => ! isSome;

    public A get { get {
      if (isSome) return __unsafeGetValue;
      throw new IllegalStateException("#get on None!");
    } }

    /* A quick way to get None instance for this options type. */
    public Option<A> none => None;

    #region Equality

    public override bool Equals(object o) => o is Option<A> option && Equals(option);

    public bool Equals(Option<A> other) => isSome ? other.exists(__unsafeGetValue) : other.isNone;

    public override int GetHashCode() => Smooth.Collections.EqComparer<A>.Default.GetHashCode(__unsafeGetValue);

    public static bool operator ==(Option<A> lhs, Option<A> rhs) {
#if ENABLE_IL2CPP
      var leftNull = ReferenceEquals(lhs, null);
      var rightNull = ReferenceEquals(rhs, null);
      if (leftNull && rightNull) return true;
      if (leftNull || rightNull) return false;
#endif
      return lhs.Equals(rhs);
    }
    
    public static bool operator !=(Option<A> lhs, Option<A> rhs) => !(lhs == rhs);

    #endregion

    public OptionEnumerator<A> GetEnumerator() => new OptionEnumerator<A>(this);

    public Option<B> map<B>(Fn<A, B> func) => 
      isSome ? F.some(func(get)) : F.none<B>();

    public Option<B> flatMap<B>(Fn<A, Option<B>> func) =>
      isSome ? func(get) : F.none<B>();

    public Option<C> flatMap<B, C>(Fn<A, Option<B>> func, Fn<A, B, C> mapper) {
      if (isNone) return Option<C>.None;
      var bOpt = func(__unsafeGetValue);
      return bOpt.isNone ? Option<C>.None : F.some(mapper(__unsafeGetValue, bOpt.__unsafeGetValue));
    }

    public override string ToString() => 
      isSome ? $"Some({__unsafeGetValue})" : "None";

    public Either<A, B> toLeft<B>(B right) => 
      isSome ? Either<A, B>.Left(__unsafeGetValue) : Either<A, B>.Right(right);

    public Either<A, B> toLeft<B>(Fn<B> right) => 
      isSome ? Either<A, B>.Left(__unsafeGetValue) : Either<A, B>.Right(right());

    public Either<B, A> toRight<B>(B left) => 
      isSome ? Either<B, A>.Right(__unsafeGetValue) : Either<B, A>.Left(left);

    public Either<B, A> toRight<B>(Fn<B> left) => 
      isSome ? Either<B, A>.Right(__unsafeGetValue) : Either<B, A>.Left(left());

    public IEnumerable<A> asEnum() => 
      isSome ? get.Yield() : Enumerable.Empty<A>();

    public Option<A> createOrTap(Fn<A> ifEmpty, Act<A> ifNonEmpty) {
      if (isNone) return new Option<A>(ifEmpty());

      ifNonEmpty(get);
      return this;
    }

    [Obsolete("Use opt1 || opt2")] public Option<A> orElse(Fn<Option<A>> other) => this || other();
    [Obsolete("Use opt1 || opt2")] public Option<A> orElse(Option<A> other) => this || other;

    public B fold<B>(Fn<B> ifEmpty, Fn<A, B> ifNonEmpty) => 
      isSome ? ifNonEmpty(get) : ifEmpty();

    public B fold<B>(B ifEmpty, Fn<A, B> ifNonEmpty) => 
      isSome ? ifNonEmpty(get) : ifEmpty;

    public B fold<B>(B ifEmpty, B ifNonEmpty) => 
      isSome ? ifNonEmpty : ifEmpty;

    public B fold<B>(B initial, Fn<A, B, B> ifNonEmpty) => 
      isSome ? ifNonEmpty(get, initial) : initial;

    // Alias for #fold with elements switched up.
    public B cata<B>(Fn<A, B> ifNonEmpty, Fn<B> ifEmpty) => 
      fold(ifEmpty, ifNonEmpty);

    // Alias for #fold with elements switched up.
    public B cata<B>(Fn<A, B> ifNonEmpty, B ifEmpty) => 
      fold(ifEmpty, ifNonEmpty);

    public Option<Tpl<A, B>> zip<B>(Option<B> opt2) => zip(opt2, F.t);

    public Option<C> zip<B, C>(Option<B> opt2, Fn<A, B, C> mapper) => 
      isSome && opt2.isSome
      ? F.some(mapper(get, opt2.get))
      : F.none<C>();

    /// <summary>
    /// If both options are Some, join them together and return Some(result).
    /// 
    /// Otherwise return that option which is Some, or None if both are None.
    /// </summary>
    public Option<A> join(Option<A> opt, Fn<A, A, A> joiner) =>
      isSome
        ? opt.isSome
          ? joiner(__unsafeGetValue, opt.__unsafeGetValue).some()
          : this
        : opt;

    /**
     * If Some() returns None. If None returns b.
     **/
    public Option<B> swap<B>(B b) => isSome ? F.none<B>() : F.some(b);
    public Option<B> swap<B>(Fn<B> b) => isSome ? F.none<B>() : F.some(b());

    public static bool operator true(Option<A> opt) => opt.isSome;

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
    public static bool operator false(Option<A> opt) => opt.isNone;

    public static Option<A> operator |(Option<A> o1, Option<A> o2) => o1 ? o1 : o2;
  }

  public struct OptionEnumerator<A> {
    public readonly Option<A> option;
    bool read;

    public OptionEnumerator(Option<A> option) : this() { this.option = option; }

    public bool MoveNext() { return option.isSome && !read; }

    public void Reset() { read = false; }

    public A Current { get {
      read = true;
      return option.get;
    } }
  }

  public static class OptionExts {
    public static ImmutableList<A> toImmutableList<A>(this Option<A> opt) => 
      opt.isSome
      ? ImmutableList.Create(opt.__unsafeGetValue)
      : ImmutableList<A>.Empty;

    public static IEnumerable<A> getOrEmpty<A>(this Option<IEnumerable<A>> enumerableOpt) =>
      enumerableOpt.isSome
      ? enumerableOpt.__unsafeGetValue
      : Enumerable.Empty<A>();
  }
}