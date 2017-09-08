﻿using System;
using com.tinylabproductions.TLPLib.Assertions;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class DateTimeExts {
    public static DateTime UNIX_EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static uint toUnixTimestamp(this DateTime dateTime) {
      dateTime.require(dateTime >= UNIX_EPOCH_START, "dateTime >= UNIX_EPOCH_START");
      return (uint) (dateTime.ToUniversalTime() - UNIX_EPOCH_START).TotalSeconds;
    }

    public static DateTime fromUnixTimestampInSeconds(this uint timestamp) {
      return UNIX_EPOCH_START.AddSeconds(timestamp);
    }

    public static DateTime fromUnixTimestampInMilliseconds(this long timestamp) {
      return UNIX_EPOCH_START.AddMilliseconds(timestamp);
    }

    public static int secondsFromNow(this DateTime d) =>
      (int)(DateTime.UtcNow - d.ToUniversalTime()).TotalSeconds;
  }
}
