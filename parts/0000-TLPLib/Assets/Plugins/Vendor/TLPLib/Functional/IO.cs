using System;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class IO {
    public static readonly IO<Unit> empty = a(() => {});

    public static IO<A> a<A>(Fn<A> fn) => new IO<A>(fn);
    public static IO<Unit> a(Action action) => new IO<Unit>(() => {
      action();
      return F.unit;
    });
  }

  /**
   * Allows encapsulating side effects and composing them.
   */
  public struct IO<A> {
    readonly Fn<A> fn;

    public IO(Fn<A> fn) { this.fn = fn; }

    public IO<B> map<B>(Fn<A, B> mapper) {
      var fn = this.fn;
      return new IO<B>(() => mapper(fn()));
    }

    public IO<B> andThen<B>(IO<B> io2) {
      var fn = this.fn;
      return new IO<B>(() => {
        fn();
        return io2.__unsafePerformIO();
      });
    }

    public IO<B> flatMap<B>(Fn<A, IO<B>> mapper) {
      var fn = this.fn;
      return new IO<B>(() => mapper(fn()).__unsafePerformIO());
    }

    /** Runs the encapsulated side effects. */
    public A __unsafePerformIO() => fn();
  }
}