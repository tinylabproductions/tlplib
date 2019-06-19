using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional.optics {
  [PublicAPI] public sealed class Lens<From, To> {
    public readonly Func<From, To> get;
    public readonly Func<From, To, From> set;

    public Lens(Func<From, To> get, Func<From, To, From> set) {
      this.get = get;
      this.set = set;
    }
    
    public Lens<From, ToA> combine<ToA>(Lens<To, ToA> lens) => new Lens<From, ToA>(
      from => lens.get(get(from)),
      (from, toA) => set(from, lens.set(get(from), toA))
    );
  }

  [PublicAPI] public static class Lens {
    public static Builder<From> from<From>() => Builder<From>.instance;

    public class Builder<From> {
      public static readonly Builder<From> instance = new Builder<From>();
      Builder() {}

      public Lens<From, To> to<To>(Func<From, To> get, Func<From, To, From> set) => 
        new Lens<From, To>(get, set);
    }
  }
}