namespace com.tinylabproductions.TLPLib.Data {
  public struct Duration {
    public readonly int millis;

    public static Duration fromSeconds(float seconds) => Duration.fromSeconds((int) (seconds * 1000));

    public Duration(int millis) { this.millis = millis; }

    public float seconds => millis / 1000f;

    public override string ToString() => $"{nameof(Duration)}({millis}ms)";
  }
}
