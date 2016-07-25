using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Net {
  public class QueryString {
    static readonly char[] equalsSeparator = {'='};

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

    public static ImmutableList<Tpl<string, string>> parseKV(string str) {
      if (str.isEmpty()) return ImmutableList<Tpl<string, string>>.Empty;

      var parts = str.Split('&');
      var list = parts.Select(part => {
        var kv = part.Split(equalsSeparator, 2);
        var key = Uri.UnescapeDataString(kv[0]);
        var value = kv.Length == 2 ? Uri.UnescapeDataString(kv[1]) : "";
        return F.t(key, value);
      }).ToImmutableList();
      return list;
    }
  }
}
