using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UIntExts {
    [PublicAPI] public static int toIntOrLog(
      this uint a,
      Log.Level level = Log.Level.ERROR,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      if (a > int.MaxValue) {
        if (Log.d.willLog(level)) Log.d.log(
          level,
          $"{nameof(UIntExts)}.{nameof(toIntOrLog)} called with {a} from " +
          $"{callerMemberName} @ {callerFilePath}:{callerLineNumber}"
        );
        return int.MaxValue;
      }

      return (int) a;
    }

    public static uint addClamped(this uint a, int b) {
      if (b < 0 && a < -b) return uint.MinValue;
      if (b > 0 && uint.MaxValue - a < b) return uint.MaxValue;
      return (uint) (a + b);
    }

    [PublicAPI] public static string toOrdinalString(this uint number) {
      var div = number % 100;
      if (div >= 11 && div <= 13) {
        return $"{number}th";
      }

      switch (number % 10) {
        case 1: return $"{number}st";
        case 2: return $"{number}nd";
        case 3: return $"{number}rd";
        default: return $"{number}th";
      }
    }
  }
}