namespace com.tinylabproductions.TLPLib.Data {
  /* Simple heap-allocated reference. */
  public class Ref<A> {
    public A value;

    public Ref(A value) {
      this.value = value;
    }
  }

  public static class Ref {
    public static Ref<A> a<A>(A value) { return new Ref<A>(value); }
  }
}
