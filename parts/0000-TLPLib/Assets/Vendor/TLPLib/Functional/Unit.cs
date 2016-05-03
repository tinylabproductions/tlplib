namespace com.tinylabproductions.TLPLib.Functional {
  public struct Unit {
    public static Unit instance => new Unit();
    public override string ToString() { return "()"; }
  }
}
