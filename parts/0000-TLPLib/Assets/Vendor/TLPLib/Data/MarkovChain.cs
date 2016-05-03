using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public static class MarkovState {
    public static MarkovState<A> a<A>(
      A data, Lazy<MarkovTransition<A>[]> transitions
    ) {
      return new MarkovState<A>(data, transitions);
    }
  }

  /** http://en.wikipedia.org/wiki/Markov_chain **/
  public class MarkovState<A> {
    public readonly A data;
    private readonly Lazy<MarkovTransition<A>[]> transitions;

    public MarkovState(
      A data, Lazy<MarkovTransition<A>[]> transitions
    ) {
      this.data = data;
      this.transitions = transitions;
    }

    public MarkovState<A> next { get {
      return transitions.get.randomElementByWeight(_ => _.weight).get.nextState;
    } }
  }

  public static class MarkovTransition {
    public static MarkovTransition<A> a<A>(MarkovState<A> nextState, int weight) {
      return new MarkovTransition<A>(nextState, weight);
    }
  }

  public class MarkovTransition<A> {
    public readonly MarkovState<A> nextState;
    public readonly int weight;

    public MarkovTransition(MarkovState<A> nextState, int weight) {
      this.nextState = nextState;
      this.weight = weight;
    }
  }
}
