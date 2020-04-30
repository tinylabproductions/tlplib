using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.reflection {
  /** Allows to create some type easily. */
  public struct Props<A> where A : class {
    public readonly Type type;

    public Props(Type type) { this.type = type; }

    public A create() => Activator.CreateInstance(type) as A;

    public Either<string, A> createE() =>
      type.hasEmptyConstructor().opt(create)
      .toRight($"{type} does not have empty args constructor!");

    public override string ToString() => $"Props<{typeof(A)}>[{type}]";
  }
}