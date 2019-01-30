using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Formats {
  public static class MiniJSONExts {
    public static Either<string, Dictionary<string, object>> asJsDict(
      this object o
    ) => o.cast().toE<Dictionary<string, object>>();

    public static Either<string, List<object>> asJsList(
      this object o
    ) => o.cast().toE<List<object>>();

    public static Either<string, string> asJsString(
      this object o
    ) => o.cast().toE<string>();
  }
}