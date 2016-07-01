namespace com.tinylabproductions.TLPLib.Extensions {
  public static class CharExts {
    public static bool isAlphabetic(this char c) =>
      (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

    public static bool isNumeric(this char c) => c >= 0 && c <= 9;

    public static bool isAlphaNumeric(this char c) => c.isAlphabetic() || c.isNumeric();
  }
}