namespace com.tinylabproductions.TLPLib.localization {
  // Localization
  public static class L10N {
    public static string toEnglish(this uint n, string singular, string plural) =>
      n == 1 ? singular : plural;
  }
}