using System;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class EncodingExts {
    public static Try<string> GetStringTry(this Encoding enc, byte[] bytes) {
      try { return F.scs(enc.GetString(bytes)); }
      catch (Exception e) { return F.err<string>(e); }
    }
  }
}