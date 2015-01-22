using System;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Functional {
  /* A or B or (A and B) */
  public struct These<A, B> {
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

    public bool isThis { get { return state == State.A; } }
    public bool isThat { get { return state == State.B; } }
    public bool isBoth { get { return state == State.BOTH; } }

    public Option<A> thisValue { get { return isThis.opt(a); } }
    public Option<B> thatValue { get { return isThat.opt(b); } }
    public Option<Tpl<A, B>> bothValue { get { return isBoth.opt(F.t(a, b)); } }

    public override string ToString() {
      return isThis ? "This(" + a + ")" 
           : isThat ? "That(" + b + ")" 
                    : "Both(" + a + ", " + b + ")";
    }

    public These<C, B> flatMapThis<C>(Fn<A, These<C, B>> mapper) {
      switch (state) {
        case State.A: 
        case State.BOTH: 
          return mapper(a);
        case State.B: return F.that<C, B>(b);
      }
      throw new IllegalStateException();
    }

    public These<A, C> flatMapThat<C>(Fn<B, These<A, C>> mapper) {
      switch (state) {
        case State.A: return F.thiz<A, C>(a);
        case State.B: 
        case State.BOTH: 
          return mapper(b);
      }
      throw new IllegalStateException();
    }

    public These<C, B> mapThis<C>(Fn<A, C> mapper) {
      switch (state) {
        case State.A: return F.thiz<C, B>(mapper(a));
        case State.B: return F.that<C, B>(b);
        case State.BOTH: return F.both(mapper(a), b);
      }
      throw new IllegalStateException();
    }

    public These<A, C> mapThat<C>(Fn<B, C> mapper) {
      switch (state) {
        case State.A: return F.thiz<A, C>(a);
        case State.B: return F.that<A, C>(mapper(b));
        case State.BOTH: return F.both(a, mapper(b));
      }
      throw new IllegalStateException();
    }

    public C fold<C>(Fn<A, C> onA, Fn<B, C> onB, Fn<A, B, C> onBoth) {
      switch (state) {
        case State.A: return onA(a);
        case State.B: return onB(b);
        case State.BOTH: return onBoth(a, b);
      }
      throw new IllegalStateException();
    }

    public void voidFold(Act<A> onA, Act<B> onB, Act<A, B> onBoth) {
      switch (state) {
        case State.A: onA(a); break;
        case State.B: onB(b); break;
        case State.BOTH: onBoth(a, b); break;
      }
      throw new IllegalStateException();
    }

    public Option<B> toOpt() { return thatValue; }

    public B getOrElse(Fn<A, B> onThis) { return isThat ? b : onThis(a); }
  }

  public static class These {
    public static Option<These<A, B>> a<A, B>(Option<A> aOpt, Option<B> bOpt) {
      if (aOpt.isEmpty && bOpt.isEmpty) return F.none<These<A, B>>();
      if (aOpt.isDefined && bOpt.isDefined) return F.both(aOpt.get, bOpt.get).some();
      if (aOpt.isDefined) return F.thiz<A, B>(aOpt.get).some();
      if (bOpt.isDefined) return F.that<A, B>(bOpt.get).some();
      throw new IllegalStateException();
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

  public struct ThisTheseBuilder<A> {
    public readonly A thisValue;

    public ThisTheseBuilder(A thisValue) {
      this.thisValue = thisValue;
    }

    public These<A, B> t<B>() { return new These<A, B>(thisValue); }
  }

  public struct ThatEitherBuilder<B> {
    public readonly B thatValue;

    public ThatEitherBuilder(B thatValue) {
      this.thatValue = thatValue;
    }

    public These<A, B> t<A>() { return new These<A, B>(thatValue); }
  }
}
