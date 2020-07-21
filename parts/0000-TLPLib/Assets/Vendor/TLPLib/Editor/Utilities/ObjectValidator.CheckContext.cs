using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    public class CheckContext {
      public static readonly CheckContext empty =
        new CheckContext(None._, ImmutableHashSet<Type>.Empty);

      public readonly Option<string> value;
      public readonly ImmutableHashSet<Type> checkedComponentTypes;

      public CheckContext(Option<string> value, ImmutableHashSet<Type> checkedComponentTypes) {
        this.value = value;
        this.checkedComponentTypes = checkedComponentTypes;
      }

      public CheckContext(string value) : this(value.some(), ImmutableHashSet<Type>.Empty) {}

      public override string ToString() => value.getOrElse("unknown ctx");

      public CheckContext withCheckedComponentType(Type c) =>
        new CheckContext(value, checkedComponentTypes.Add(c));
    }
  }
}