namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxRefOps {
    public static IRxVal<A> asVal<A>(this IRxRef<A> rx) => rx;
  }
}