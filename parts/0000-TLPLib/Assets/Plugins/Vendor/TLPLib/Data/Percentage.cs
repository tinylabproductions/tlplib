namespace com.tinylabproductions.TLPLib.Data {
  public struct Percentage {
    // [0, 1]
    public readonly float value;

    public Percentage(float value) {
      this.value = value;
    }

    public override string ToString() => $"{nameof(Percentage)}({value})";
  }
}