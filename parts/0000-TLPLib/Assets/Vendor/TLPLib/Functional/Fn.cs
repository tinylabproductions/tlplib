namespace com.tinylabproductions.TLPLib.Functional {
  public static class Fn {
    public static A1 keep1stArg<A1, A2>(A1 a1, A2 a2) => a1;
    public static A2 keep2ndArg<A1, A2>(A1 a1, A2 a2) => a2;
  }
}