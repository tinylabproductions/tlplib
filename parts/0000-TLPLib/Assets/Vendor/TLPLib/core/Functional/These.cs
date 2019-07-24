using System;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Functional {
  /* A or B or (A and B) */
  public class These<A, B> {
    enum State { A, B, BOTH }

    readonly A a;
    readonly B b;
    readonly State state;

    public These(A a) {
      this.a = a;
      b = default(B);
      state = State.A;
    }

    public These(B b) {
      a = default(A);
      this.b = b;
      state = State.B;
    }

    public These(A a, B b) {
      this.a = a;
      this.b = b;
      state = State.BOTH;
    }

    public bool isThis => state == State.A;
    public bool isThat => state == State.B;
    public bool isBoth => state == State.BOTH;

    public Option<A> thisValue => (isThis || isBoth).opt(a);
    public Option<B> thatValue => (isThat || isBoth).opt(b);
    public Option<Tpl<A, B>> bothValue => isBoth.opt(F.t(a, b));

    public override string ToString() {
      return isThis ? $"This({a})"
           : isThat ? $"That({b})"
                    : $"Both({a}, {b})";
    }

    public These<AA, B> flatMapThis<AA>(Func<A, These<AA, B>> mapper) {
      switch (state) {
        case State.A:
        case State.BOTH:
          return mapper(a);
        case State.B: return F.that<AA, B>(b);
        default: throw new IllegalStateException();
      }
    }

    public These<A, BB> flatMapThat<BB>(Func<B, These<A, BB>> mapper) {
      switch (state) {
        case State.A: return F.thiz<A, BB>(a);
        case State.B:
        case State.BOTH:
          return mapper(b);
        default: throw new IllegalStateException();
      }
    }

    public These<AA, B> mapThis<AA>(Func<A, AA> mapper) {
      switch (state) {
        case State.A: return F.thiz<AA, B>(mapper(a));
        case State.B: return F.that<AA, B>(b);
        case State.BOTH: return F.these(mapper(a), b);
        default: throw new IllegalStateException();
      }
    }

    public These<A, BB> mapThat<BB>(Func<B, BB> mapper) {
      switch (state) {
        case State.A: return F.thiz<A, BB>(a);
        case State.B: return F.that<A, BB>(mapper(b));
        case State.BOTH: return F.these(a, mapper(b));
        default: throw new IllegalStateException();
      }
    }

    public These<AA, BB> map<AA, BB>(Func<A, AA> aMapper, Func<B, BB> bMapper) {
      switch (state) {
        case State.A: return F.thiz<AA, BB>(aMapper(a));
        case State.B: return F.that<AA, BB>(bMapper(b));
        case State.BOTH: return F.these(aMapper(a), bMapper(b));
        default: throw new IllegalStateException();
      }
    }

    public C fold<C>(Func<A, C> onA, Func<B, C> onB, Func<A, B, C> onBoth) {
      switch (state) {
        case State.A: return onA(a);
        case State.B: return onB(b);
        case State.BOTH: return onBoth(a, b);
        default: throw new IllegalStateException();
      }
    }

    public void voidFold(Action<A> onA, Action<B> onB, Action<A, B> onBoth) {
      switch (state) {
        case State.A: onA(a); break;
        case State.B: onB(b); break;
        case State.BOTH: onBoth(a, b); break;
        default: throw new IllegalStateException();
      }
    }

    public Option<B> toOpt() { return thatValue; }

    public B getOrElse(Func<A, B> onThis) { return isThat ? b : onThis(a); }

    public These<AA, B> withThis<AA>(AA a) {
      switch (state) {
        case State.A: return new These<AA, B>(a);
        case State.B: return new These<AA, B>(b);
        case State.BOTH: return new These<AA, B>(a, b);
        default: throw new IllegalStateException();
      }
    }

    public These<A, BB> withThat<BB>(BB b) {
      switch (state) {
        case State.A: return new These<A, BB>(a);
        case State.B: return new These<A, BB>(b);
        case State.BOTH: return new These<A, BB>(a, b);
        default: throw new IllegalStateException();
      }
    }
  }

  public static class These {
    public static Option<C> a<A, B, C>(Option<A> aOpt, Option<B> bOpt, Func<These<A, B>, C> mapper) {
      if (aOpt.isNone && bOpt.isNone) return F.none_;
      if (aOpt.isSome && bOpt.isSome) return mapper(F.these(aOpt.__unsafeGet, bOpt.__unsafeGet)).some();
      if (aOpt.isSome) return mapper(F.thiz<A, B>(aOpt.get)).some();
      if (bOpt.isSome) return mapper(F.that<A, B>(bOpt.get)).some();
      throw new IllegalStateException();
    }

    public static Option<These<A, B>> a<A, B>(Option<A> aOpt, Option<B> bOpt) =>
      a(aOpt, bOpt, _ => _);

    // TODO: test
    public static Tpl<A, Option<A>> asTuple<A>(this These<A, A> t) {
      if (t.isBoth) return F.t(t.thisValue.get, t.thatValue);
      if (t.isThis) return F.t(t.thisValue.get, F.none<A>());
      if (t.isThat) return F.t(t.thatValue.get, F.none<A>());
      throw new IllegalStateException($"Unknown case of {t}");
    }
  }

  public static class TheseBuilderExts {
    public static ThisTheseBuilder<A> thiz<A>(this A value) {
      return new ThisTheseBuilder<A>(value);
    }

    public static ThatEitherBuilder<B> that<B>(this B value) {
      return new ThatEitherBuilder<B>(value);
    }

    public static These<A, B> both<A, B>(this A a, B b) {
      return new These<A, B>(a, b);
    }
  }

  public class ThisTheseBuilder<A> {
    public readonly A thisValue;

    public ThisTheseBuilder(A thisValue) {
      this.thisValue = thisValue;
    }

    public These<A, B> t<B>() { return new These<A, B>(thisValue); }
  }

  public class ThatEitherBuilder<B> {
    public readonly B thatValue;

    public ThatEitherBuilder(B thatValue) {
      this.thatValue = thatValue;
    }

    public These<A, B> t<A>() { return new These<A, B>(thatValue); }
  }
}