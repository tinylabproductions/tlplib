using System;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class StringExts {
    public static Either<string, int> parseInt(this String str) {
      int output;
      return int.TryParse(str, out output).either(
        () => "cannot parse as int: '" + str + "'", output
      );
    }

    public static Either<string, uint> parseUInt(this String str) {
      uint output;
      return uint.TryParse(str, out output).either(
        () => "cannot parse as uint: '" + str + "'", output
      );
    }

    public static Either<string, long> parseLong(this String str) {
      long output;
      return long.TryParse(str, out output).either(
        () => "cannot parse as long: '" + str + "'", output
      );
    }

    public static Either<string, float> parseFloat(this String str) {
      float output;
      return float.TryParse(str, out output).either(
        () => "cannot parse as float: '" + str + "'", output
      );
    }

    public static Either<string, double> parseDouble(this String str) {
      double output;
      return double.TryParse(str, out output).either(
        () => "cannot parse as double: '" + str + "'", output
      );
    }

    public static Either<string, bool> parseBool(this String str) {
      bool output;
      return bool.TryParse(str, out output).either(
        () => "cannot parse as bool: '" + str + "'", output
      );
    }

    public static Either<Exception, DateTime> parseDateTime(this String str) {
      try {
        return F.right<Exception, DateTime>(DateTime.Parse(str));
      }
      catch (Exception e) {
        return F.left<Exception, DateTime>(e);
      }
    }
  }
}
