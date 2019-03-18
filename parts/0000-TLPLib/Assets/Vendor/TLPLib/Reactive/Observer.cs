namespace com.tinylabproductions.TLPLib.Reactive {
  public interface IRxObserver<in A> {
    void push(A value);
  }
}