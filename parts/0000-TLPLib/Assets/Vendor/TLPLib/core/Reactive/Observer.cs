namespace com.tinylabproductions.TLPLib.Reactive {
  public interface IObserver<in A> {
    void push(A value);
  }
}