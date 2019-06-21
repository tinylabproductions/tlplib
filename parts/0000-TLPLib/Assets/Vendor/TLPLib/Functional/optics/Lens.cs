using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional.optics {
  [PublicAPI] public sealed class Lens<From, To> {
    public delegate To Get(From from);
    public delegate From Set(From on, To value);
    
    public readonly Get get;
    public readonly Set set;

    public Lens(Get get, Set set) {
      this.get = get;
      this.set = set;
    }
    
    public Lens<From, ToA> combine<ToA>(Lens<To, ToA> lens) => new Lens<From, ToA>(
      from => lens.get(get(from)),
      (from, toA) => set(from, lens.set(get(from), toA))
    );
    
    public Prism<From, ToA> combine<ToA>(Prism<To, ToA> prism) => new Prism<From, ToA>(
      from => prism.get(get(from)),
      (from, toA) => {
        var toEither = prism.set(get(@from), toA);
        if (toEither.rightValueOut(out var to2)) return set(from, to2);
        else return toEither.__unsafeGetLeft;
      }
    );
  }

  [PublicAPI] public static class Lens {
    public static Builder<From> from<From>() => Builder<From>.instance;

    [PublicAPI] public class Builder<From> {
      public static readonly Builder<From> instance = new Builder<From>();
      Builder() {}

      public Lens<From, To> to<To>(Lens<From, To>.Get get, Lens<From, To>.Set set) => 
        new Lens<From, To>(get, set);
    }
  }
}