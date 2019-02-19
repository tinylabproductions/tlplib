using System;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class IO {
    public static readonly IO<Unit> empty = a(() => {});

    public static IO<A> a<A>(Func<A> fn) => new IO<A>(fn);
    public static IO<Unit> a(Action action) => new IO<Unit>(() => {
      action();
      return F.unit;
    });
  }

  /**
   * Allows encapsulating side effects and composing them.
   */
  public struct IO<A> {
    readonly Func<A> fn;

    public IO(Func<A> fn) { this.fn = fn; }

    public IO<B> map<B>(Func<A, B> mapper) {
      var fn = this.fn;
      return new IO<B>(() => mapper(fn()));
    }

    /// <summary>Compose both IOs, ignoring the result of the first one.</summary>
    public IO<B> andThen<B>(IO<B> io2) {
      var fn = this.fn;
      return new IO<B>(() => {
        fn();
        return io2.__unsafePerformIO();
      });
    }

    public IO<B> flatMap<B>(Func<A, IO<B>> mapper) {
      var fn = this.fn;
      return new IO<B>(() => mapper(fn()).__unsafePerformIO());
    }

    public IO<B1> flatMap<B, B1>(Func<A, IO<B>> mapper, Func<A, B, B1> g) {
      var fn = this.fn;
      return new IO<B1>(() => {
        var a = fn();
        var b = mapper(a).__unsafePerformIO();
        return g(a, b);
      });
    }

    /// <summary>Runs the encapsulated side effects.</summary>
    public A __unsafePerformIO() => fn();

    /// <summary>Alias for <see cref="andThen{B}"/></summary>
    public static IO<A> operator +(IO<A> io1, IO<A> io2) => io1.andThen(io2);
  }
}