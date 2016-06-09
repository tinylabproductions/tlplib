namespace com.tinylabproductions.TLPLib.Data {
  public struct Duration {
    public readonly float seconds;

    public Duration(float seconds) { this.seconds = seconds; }

    public int millis => (int) (seconds * 1000);

    public override string ToString() => $"{nameof(Duration)}({seconds}s)";
  }
}
