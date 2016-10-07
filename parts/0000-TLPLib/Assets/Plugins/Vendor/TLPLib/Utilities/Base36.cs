using System;
using System.Text;

namespace com.tinylabproductions.TLPLib.Utilities {
  public class Base36 {
    private const string Clist = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly char[] Clistarr = Clist.ToCharArray();

    public static long Decode(string inputString) {
      long result = 0;
      var pow = 0;
      for (var i = inputString.Length - 1; i >= 0; i--) {
        var c = inputString[i];
        var pos = Clist.IndexOf(c);
        if (pos > -1)
          result += pos * (long)Math.Pow(Clist.Length, pow);
        else
          return -1;
        pow++;
      }
      return result;
    }

    public static string Encode(ulong inputNumber) {
      var sb = new StringBuilder();
      do {
        sb.Append(Clistarr[inputNumber % (ulong)Clist.Length]);
        inputNumber /= (ulong)Clist.Length;
      } while (inputNumber != 0);
      return Reverse(sb.ToString());
    }

    public static string Reverse(string s) {
      var charArray = s.ToCharArray();
      Array.Reverse(charArray);
      return new string(charArray);
    }
  }
}
