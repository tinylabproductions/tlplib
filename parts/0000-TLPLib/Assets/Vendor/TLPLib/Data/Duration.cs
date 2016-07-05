namespace com.tinylabproductions.TLPLib.Data {
  public struct Duration {
    public readonly int millis;

    public static Duration fromSeconds(int seconds) => new Duration(seconds * 1000);
    public static Duration fromSeconds(float seconds) => new Duration((int) (seconds * 1000));

    public Duration(int millis) { this.millis = millis; }

    public float seconds => millis / 1000f;

    public override string ToString() => $"{nameof(Duration)}({millis}ms)";
  }

  public static class DurationExts {
    public static Duration milli(this int v) => v.millis();
    public static Duration millis(this int v) => new Duration(v);

    public static Duration second(this int v) => v.seconds();
    public static Duration second(this float v) => v.seconds();
    public static Duration seconds(this int v) => Duration.fromSeconds(v);
    public static Duration seconds(this float v) => Duration.fromSeconds(v);

    public static Duration minute(this int v) => v.minutes();
    public static Duration minute(this float v) => v.minutes();
    public static Duration minutes(this int v) => Duration.fromSeconds(v * 60);
    public static Duration minutes(this float v) => Duration.fromSeconds(v * 60);

    public static Duration hour(this int v) => v.hours();
    public static Duration hour(this float v) => v.hours();
    public static Duration hours(this int v) => Duration.fromSeconds(v * 3600);
    public static Duration hours(this float v) => Duration.fromSeconds(v * 3600);
  }
}