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

    public static string limitLength(this string str, int length, string cutoff="...") 
    { return str.Length <= length ? str : str.Substring(0, length) + cutoff; }
  }
}
