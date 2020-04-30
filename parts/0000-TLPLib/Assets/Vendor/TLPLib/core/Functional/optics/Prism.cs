using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Functional.optics {
  [PublicAPI] public sealed class Prism<From, To> {
    public delegate Either<string, To> Get(From from);
    public delegate Either<string, From> Set(From on, To value);
    
    public readonly Get get;
    public readonly Set set;

    public Prism(Get get, Set set) {
      this.get = get;
      this.set = set;
    }
    
    public Prism<From, ToA> combine<ToA>(Prism<To, ToA> prism) => new Prism<From, ToA>(
      from => {
        var toEither = get(from);
        return toEither.rightValueOut(out var to) ? prism.get(to) : toEither.__unsafeGetLeft;
      },
      (on, toAValue) => {
        var toEither1 = get(on);
        if (toEither1.rightValueOut(out var to1)) {
          var toEither2 = prism.set(to1, toAValue);
          return toEither2.rightValueOut(out var to2) ? set(on, to2) : toEither2.__unsafeGetLeft;
        }
        else {
          return toEither1.__unsafeGetLeft;
        }
      }
    );
    
    public Prism<From, ToA> combine<ToA>(Lens<To, ToA> lens) => new Prism<From, ToA>(
      from => {
        var toEither = get(from);
        return toEither.rightValueOut(out var to) ? (Either<string, ToA>) lens.get(to) : toEither.__unsafeGetLeft;
      },
      (on, toAValue) => {
        var toEither1 = get(on);
        return toEither1.rightValueOut(out var to1) ? set(@on, lens.set(to1, toAValue)) : toEither1.__unsafeGetLeft;
      }
    );
  }

  [PublicAPI] public static class Prism {
    public static Builder<From> from<From>() => Builder<From>.instance;

    [PublicAPI] public class Builder<From> {
      public static readonly Builder<From> instance = new Builder<From>();
      Builder() { }

      public Prism<From, To> to<To>(Prism<From, To>.Get get, Prism<From, To>.Set set) =>
        new Prism<From, To>(get, set);
    }
  }
}