using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional.higher_kinds;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Functional {
  [PublicAPI] public static class Option {
    // Witness for higher kinds
    public struct W {}

    public static Option<A> narrowK<A>(this HigherKind<W, A> hkt) => (Option<A>) hkt;

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
    public static A getOrUpdate<A>(ref Option<A> opt, Func<A> create) {
      if (opt.isNone) opt = new Option<A>(create());
      return opt.__unsafeGetValue;
    }
  }

  [PublicAPI] public struct None {
    public static None i => new None();
  }
  
  [PublicAPI] public
#if ENABLE_IL2CPP
    sealed class
#else
    readonly struct
#endif
    Option<A> : HigherKind<Option.W, A> 
  {
    public static readonly Option<A> None
#if ENABLE_IL2CPP
      = new Option<A>();
#endif

    public readonly A __unsafeGetValue;
    public readonly bool isSome;
    public bool isNone => !isSome;
      
#if ENABLE_IL2CPP
    Option() {}
#endif
    
    public Option(A value) : this() {
      __unsafeGetValue = value;
      isSome = true;
    }

    public A get { get {
      if (isSome) return __unsafeGetValue;
      throw new Exception("#get on None!");
    } }

    #region Equality

    public override bool Equals(object o) => o is Option<A> option && Equals(option);

    public bool Equals(Option<A> other) => isSome ? other.exists(__unsafeGetValue) : other.isSome;

    public override int GetHashCode() => EqComparer<A>.Default.GetHashCode(__unsafeGetValue);

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

    public override string ToString() =>
      isSome ? $"Some({__unsafeGetValue})" : "None";

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

    /// <summary>Allows implicitly converting <see cref="None"/> to None <see cref="Option{A}"/>.</summary>
    public static implicit operator Option<A>(None _) => None;
  }

  [PublicAPI] public struct OptionEnumerator<A> {
    public readonly Option<A> option;
    bool read;

    public OptionEnumerator(Option<A> option) : this() { this.option = option; }

    public bool MoveNext() => option.isSome && !read;

    public void Reset() => read = false;

    public A Current { get {
      read = true;
      return option.get;
    } }
  }

  [PublicAPI] public static class OptionExts {
    public static IEnumerable<Base> asEnum<Base, Child>(this Option<Child> opt) where Child : Base =>
      opt.isSome ? new Base[] {opt.__unsafeGetValue} : Enumerable.Empty<Base>();

    public static A getOrNull<A>(this Option<A> opt) where A : class =>
      opt.isSome ? opt.get : null;

    public static A orNull<A>(this Option<A> opt) where A : class =>
      opt.getOrNull();

    public static Option<A> flatten<A>(this Option<Option<A>> opt) =>
      opt.isSome ? opt.__unsafeGetValue : Option<A>.None;

    public static Option<Base> cast<Child, Base>(this Option<Child> o) where Child : Base => 
      o.isSome ? new Option<Base>(o.__unsafeGetValue) : Option<Base>.None;

    public static A getOrElse<A>(this Option<A> opt, Func<A> orElse) {
#if ENABLE_IL2CPP
      if (opt == null) return orElse();
#endif
      return opt.isSome ? opt.__unsafeGetValue : orElse();
    }

    public static A getOrElse<A>(this Option<A> opt, A orElse) {
#if ENABLE_IL2CPP
      if (opt == null) return orElse;
#endif
      return opt.isSome ? opt.__unsafeGetValue : orElse;
    }

    public static bool valueOut<A>(this Option<A> maybeA, out A a) {
      a = maybeA.isSome ? maybeA.__unsafeGetValue : default;
      return maybeA.isSome;
    }

    public static Option<A> fromNullable<A>(this A? maybeA) where A : struct =>
      maybeA.HasValue ? new Option<A>(maybeA.Value) : Option<A>.None;
    
    public static A? toNullable<A>(this Option<A> maybeA) where A : struct =>
      maybeA.isSome ? maybeA.__unsafeGetValue : (A?) null;
    
    public static A getOrThrow<A>(this Option<A> maybeA, Func<Exception> getEx) =>
      maybeA.isSome ? maybeA.__unsafeGetValue : throw getEx();

    public static A getOrThrow<A>(this Option<A> maybeA, string message) =>
      maybeA.isSome ? maybeA.__unsafeGetValue : throw new Exception(message);

    public static void voidFold<A>(this Option<A> maybeA, Action ifEmpty, Action<A> ifNonEmpty) {
      if (maybeA.isSome) ifNonEmpty(maybeA.__unsafeGetValue);
      else ifEmpty();
    }

    public static Option<A> filter<A>(this Option<A> maybeA, bool keepValue) =>
      keepValue ? maybeA : Option<A>.None;

    public static Option<A> filter<A>(this Option<A> maybeA, Func<A, bool> predicate) =>
      maybeA.isSome && predicate(maybeA.__unsafeGetValue) ? maybeA : F.none<A>();

    public static bool exists<A>(this Option<A> maybeA, Func<A, bool> predicate) =>
      maybeA.isSome && predicate(maybeA.__unsafeGetValue);

    public static bool exists<A>(this Option<A> maybeA, A a) =>
      exists(maybeA, a, EqualityComparer<A>.Default);

    public static bool exists<A>(this Option<A> maybeA, A a, IEqualityComparer<A> comparer) =>
      maybeA.isSome && comparer.Equals(maybeA.__unsafeGetValue, a);
    
    public static Option<B> map<A, B>(this Option<A> maybeA, Func<A, B> func) =>
      maybeA.isSome ? new Option<B>(func(maybeA.__unsafeGetValue)) : Option<B>.None;

    public static Option<B> flatMap<A, B>(this Option<A> maybeA, Func<A, Option<B>> func) =>
      maybeA.isSome ? func(maybeA.__unsafeGetValue) : Option<B>.None;

    public static Option<B> flatMapUnity<A, B>(this Option<A> maybeA, Func<A, B> func) where B : class =>
      maybeA.isSome ? F.opt(func(maybeA.__unsafeGetValue)) : Option<B>.None;

    public static Option<C> flatMap<A, B, C>(
      this Option<A> maybeA, Func<A, Option<B>> func, Func<A, B, C> mapper
    ) {
      if (maybeA.isNone) return Option<C>.None;
      var bOpt = func(maybeA.__unsafeGetValue);
      return bOpt.isNone ? Option<C>.None : F.some(mapper(maybeA.__unsafeGetValue, bOpt.__unsafeGetValue));
    }

    public static Either<A, B> toLeft<A, B>(this Option<A> maybeA, B right) =>
      maybeA.isSome ? Either<A, B>.Left(maybeA.__unsafeGetValue) : Either<A, B>.Right(right);

    public static Either<A, B> toLeft<A, B>(this Option<A> maybeA, Func<B> right) =>
      maybeA.isSome ? Either<A, B>.Left(maybeA.__unsafeGetValue) : Either<A, B>.Right(right());

    public static Either<B, A> toRight<A, B>(this Option<A> maybeA, B left) =>
      maybeA.isSome ? Either<B, A>.Right(maybeA.__unsafeGetValue) : Either<B, A>.Left(left);

    public static Either<B, A> toRight<A, B>(this Option<A> maybeA, Func<B> left) =>
      maybeA.isSome ? Either<B, A>.Right(maybeA.__unsafeGetValue) : Either<B, A>.Left(left());

    public static IEnumerable<A> asEnum<A>(this Option<A> maybeA) =>
      maybeA.isSome ? maybeA.__unsafeGetValue.Yield() : Enumerable.Empty<A>();

    public static Option<A> createOrTap<A>(this Option<A> maybeA, Func<A> ifEmpty, Action<A> ifNonEmpty) {
      if (maybeA.isNone) return new Option<A>(ifEmpty());

      ifNonEmpty(maybeA.__unsafeGetValue);
      return maybeA;
    }

    public static B fold<A, B>(this Option<A> maybeA, Func<B> ifEmpty, Func<A, B> ifNonEmpty) =>
      maybeA.isSome ? ifNonEmpty(maybeA.__unsafeGetValue) : ifEmpty();

    public static B fold<A, B>(this Option<A> maybeA, B ifEmpty, Func<A, B> ifNonEmpty) =>
      maybeA.isSome ? ifNonEmpty(maybeA.__unsafeGetValue) : ifEmpty;

    public static B fold<A, B>(this Option<A> maybeA, B ifEmpty, B ifNonEmpty) =>
      maybeA.isSome ? ifNonEmpty : ifEmpty;

    public static B fold<A, B>(this Option<A> maybeA, B initial, Func<A, B, B> ifNonEmpty) =>
      maybeA.isSome ? ifNonEmpty(maybeA.__unsafeGetValue, initial) : initial;

    public static Option<Tpl<A, B>> zip<A, B>(this Option<A> maybeA, Option<B> opt2) => 
      maybeA.zip(opt2, F.t);

    public static Option<C> zip<A, B, C>(this Option<A> maybeA, Option<B> opt2, Func<A, B, C> mapper) =>
      maybeA.isSome && opt2.isSome
      ? F.some(mapper(maybeA.__unsafeGetValue, opt2.__unsafeGetValue))
      : F.none<C>();

    /// <summary>
    /// If both options are Some, join them together and return Some(result).
    ///
    /// Otherwise return that option which is Some, or None if both are None.
    /// </summary>
    public static Option<A> join<A>(this Option<A> maybeA, Option<A> opt, Func<A, A, A> joiner) =>
      maybeA.isSome
        ? opt.isSome
          ? joiner(maybeA.__unsafeGetValue, opt.__unsafeGetValue).some()
          : maybeA
        : opt;

    /// <summary>If Some() returns None. If None returns b.</summary>
    public static Option<B> swap<A, B>(this Option<A> maybeA, B b) => 
      maybeA.isSome ? Option<B>.None : F.some(b);
    
    public static Option<B> swap<A, B>(this Option<A> maybeA, Func<B> b) => 
      maybeA.isSome ? Option<B>.None : F.some(b());
    
    public static ImmutableList<A> toImmutableList<A>(this Option<A> opt) =>
      opt.isSome
      ? ImmutableList.Create(opt.__unsafeGetValue)
      : ImmutableList<A>.Empty;

    public static IEnumerable<A> getOrEmpty<A>(this Option<IEnumerable<A>> enumerableOpt) =>
      enumerableOpt.isSome
      ? enumerableOpt.__unsafeGetValue
      : Enumerable.Empty<A>();

    public static Option<A> toOption<A>(this A? maybeA) where A : struct => 
      maybeA.HasValue ? new Option<A>(maybeA.Value) : Option<A>.None;
    
    public static bool getOrLog<A>(
      this Option<A> maybeA, out A a, LogEntry msg, ILog log = null, Log.Level level = Log.Level.ERROR
    ) {
      if (!maybeA.valueOut(out a)) {
        log = log ?? Log.d;
        if (log.willLog(level)) log.log(level, msg);
        return false;
      }
      return true;
    }
  }
}