﻿namespace com.tinylabproductions.TLPLib.Extensions {
  public static class IntExts {
    public static long toLong(this int i) => i;

    public static string toMinSecString(this int seconds) => $"{seconds / 60}:{seconds % 60:00}";
  }
}