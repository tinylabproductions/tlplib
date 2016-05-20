namespace com.tinylabproductions.TLPLib.Functional {
  public struct Unit {
    public static Unit instance { get; } = new Unit();
    public override string ToString() { return "()"; }
  }
}
