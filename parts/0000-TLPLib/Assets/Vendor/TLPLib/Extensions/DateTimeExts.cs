using System;
using com.tinylabproductions.TLPLib.Assertions;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class DateTimeExts {
    public static DateTime UNIX_EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [PublicAPI] public static uint toUnixTimestamp(this DateTime dateTime) {
      dateTime.require(dateTime >= UNIX_EPOCH_START, "dateTime >= UNIX_EPOCH_START");
      return (uint) (dateTime.ToUniversalTime() - UNIX_EPOCH_START).TotalSeconds;
    }

    [PublicAPI] public static long toUnixTimestampInMilliseconds(this DateTime dateTime) {
      dateTime.require(dateTime >= UNIX_EPOCH_START, "dateTime >= UNIX_EPOCH_START");
      return (long) (dateTime.ToUniversalTime() - UNIX_EPOCH_START).TotalMilliseconds;
    }

    [PublicAPI] public static DateTime fromUnixTimestampInSeconds(this uint timestamp) => 
      UNIX_EPOCH_START.AddSeconds(timestamp);

    [PublicAPI] public static DateTime fromUnixTimestampInSeconds(this long timestamp) =>
      UNIX_EPOCH_START.AddSeconds(timestamp);

    [PublicAPI] public static DateTime fromUnixTimestampInMilliseconds(this long timestamp) => 
      UNIX_EPOCH_START.AddMilliseconds(timestamp);

    [PublicAPI] public static int secondsFromNow(this DateTime d) =>
      (int)(DateTime.UtcNow - d.ToUniversalTime()).TotalSeconds;
  }
}
