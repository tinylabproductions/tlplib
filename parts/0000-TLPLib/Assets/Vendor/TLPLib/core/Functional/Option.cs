// using System;
// using System.Collections.Generic;
// using System.Collections.Immutable;
// using System.Diagnostics;
// using System.Linq;
// using com.tinylabproductions.TLPLib.Extensions;
// using com.tinylabproductions.TLPLib.Functional.higher_kinds;
// using com.tinylabproductions.TLPLib.Logger;


// using JetBrains.Annotations;
// using pzd.lib.functional;
//
// namespace com.tinylabproductions.TLPLib.Functional {
//   [PublicAPI] public static class Option {
//
//
//     /// <summary>
//     /// Example usage:
//     ///
//     /// <code><![CDATA[
//     /// Option<NonEmpty<ImmutableList<DraggablePart>>> __parts =
//     ///   None._;
//     /// public NonEmpty<ImmutableList<DraggablePart>> parts =>
//     ///   Option.getOrUpdate(ref __parts, () => _parts.ToImmutableList().toNonEmpty().get);
//     /// ]]></code>
//     /// </summary>
//     public static A getOrUpdate<A>(ref Option<A> opt, Func<A> create) {
//       if (opt.isNone) opt = new Option<A>(create());
//       return opt.__unsafeGet;
//     }
//
//     public static IEnumerable<Base> asEnum<Base, Child>(this Option<Child> opt) where Child : Base =>
//       opt.isSome ? ((Base) opt.get).yield() : Enumerable.Empty<Base>();
//
//     public static A getOrNull<A>(this Option<A> opt) where A : class =>
//       opt.isSome ? opt.get : null;
//
//     public static A orNull<A>(this Option<A> opt) where A : class =>
//       opt.getOrNull();
//
//     public static Option<A> flatten<A>(this Option<Option<A>> opt) =>
//       opt.isSome ? opt.__unsafeGet : F.none<A>();
//
//     public static Option<Base> cast<Child, Base>(this Option<Child> o) where Child : Base
//       { return o.isSome ? F.some((Base) o.get) : F.none<Base>(); }
//   }
//   
//   [PublicAPI] public
// #if ENABLE_IL2CPP
//     sealed class
// #else
//     struct
// #endif
//     Option<A> : HigherKind<Option.W, A>
//   {
//     public static None._ { get; } = new Option<A>();
//
//     public readonly A __unsafeGet;
//     public readonly bool isSome;
//
//     public Option(A value) : this() {
//       __unsafeGet = value;
//       isSome = true;
//     }
//
//     public Option<A> filter(bool keepValue) =>
//       keepValue ? this : F.none<A>();
//
//     public Option<A> filter(Func<A, bool> predicate) =>
//       isSome && predicate(__unsafeGet) ? this : F.none<A>();
//
//     public bool isNone => ! isSome;
//
//     public A get { get {
//       if (isSome) return __unsafeGet;
//       throw new IllegalStateException("#get on None!");
//     } }
//
//     public bool valueOut(out A a) {
//       a = isSome ? __unsafeGet : default;
//       return isSome;
//     }
//
//     public Option<B> map<B>(Func<A, B> func) =>
//       isSome ? F.some(func(__unsafeGet)) : F.none<B>();
//
//     public Option<B> flatMap<B>(Func<A, Option<B>> func) =>
//       isSome ? func(__unsafeGet) : F.none<B>();
//
//     public Option<C> flatMap<B, C>(Func<A, Option<B>> func, Func<A, B, C> mapper) {
//       if (isNone) return None._;
//       var bOpt = func(__unsafeGet);
//       return bOpt.isNone ? None._ : F.some(mapper(__unsafeGet, bOpt.__unsafeGet));
//     }
//
//     public B fold<B>(Func<B> ifEmpty, Func<A, B> ifNonEmpty) =>
//       isSome ? ifNonEmpty(get) : ifEmpty();
//
//     public B fold<B>(B ifEmpty, Func<A, B> ifNonEmpty) =>
//       isSome ? ifNonEmpty(get) : ifEmpty;
//
//     public B fold<B>(B ifEmpty, B ifNonEmpty) =>
//       isSome ? ifNonEmpty : ifEmpty;
//
//     public B fold<B>(B initial, Func<A, B, B> ifNonEmpty) =>
//       isSome ? ifNonEmpty(get, initial) : initial;
//
//     /// <summary>
//     /// If both options are Some, join them together and return Some(result).
//     ///
//     /// Otherwise return that option which is Some, or None if both are None.
//     /// </summary>
//     public Option<A> join(Option<A> opt, Func<A, A, A> joiner) =>
//       isSome
//         ? opt.isSome
//           ? joiner(__unsafeGet, opt.__unsafeGet).some()
//           : this
//         : opt;
//
//     /**
//      * If Some() returns None. If None returns b.
//      **/
//     public Option<B> swap<B>(B b) => isSome ? F.none<B>() : F.some(b);
//     public Option<B> swap<B>(Func<B> b) => isSome ? F.none<B>() : F.some(b());
//
//     public static implicit operator pzd.lib.functional.Option<A>(Option<A> o) => 
//       o.isSome ? new pzd.lib.functional.Option<A>(o.__unsafeGet) : pzd.lib.functional.None._;
//     public static implicit operator Option<A>(pzd.lib.functional.Option<A> o) => 
//       o.isSome ? new Option<A>(o.__unsafeGet) : None;
//   }
//
//   [PublicAPI] public static class OptionExts {
//     public static ImmutableList<A> toImmutableList<A>(this Option<A> opt) =>
//       opt.isSome
//       ? ImmutableList.Create(opt.__unsafeGet)
//       : ImmutableList<A>.Empty;
//
//     public static Option<A> toOption<A>(this A? maybeA) where A : struct => 
//       maybeA.HasValue ? new Option<A>(maybeA.Value) : None._;
//   }
// }