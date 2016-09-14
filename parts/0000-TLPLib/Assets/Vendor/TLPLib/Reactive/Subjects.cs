namespace com.tinylabproductions.TLPLib.Reactive {
  /** 
   * A subject is something that is Observable and Observer at the same
   * time.
   **/
  public class Subject<A> : Observable<A>, IObserver<A> {
    public void push(A value) => submit(value);
    public void finish() => finishObservable();
  }
}
