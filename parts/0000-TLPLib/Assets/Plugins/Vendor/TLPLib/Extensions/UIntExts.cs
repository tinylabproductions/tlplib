namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UIntExts {
    public static int toIntClamped(this uint a) => 
      a > int.MaxValue ? int.MaxValue : (int) a;

    public static uint addClamped(this uint a, int b) {
      if (b < 0 && a < -b) return uint.MinValue;
      if (b > 0 && uint.MaxValue - a < b) return uint.MaxValue;
      return (uint) (a + b);
    }

    public static string toOrdinalString(this uint number) {
      var div = number % 100;
      if ((div >= 11) && (div <= 13)) {
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