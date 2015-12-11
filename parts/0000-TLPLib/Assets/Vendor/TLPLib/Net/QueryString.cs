using System;
using System.Collections.Generic;
using System.Text;

namespace com.tinylabproductions.TLPLib.Net {
  public class QueryString {
    public static string build(IEnumerable<Tpl<string, string>> qsParams) {
      var sb = new StringBuilder();
      var isFirst = true;
      foreach (var qsParam in qsParams) {
        if (isFirst) isFirst = false;
        else sb.Append("&");
        sb.Append(Uri.EscapeDataString(qsParam._1));
        sb.Append("=");
        sb.Append(Uri.EscapeDataString(qsParam._2));
      }
      return sb.ToString();
    }
  }
}
