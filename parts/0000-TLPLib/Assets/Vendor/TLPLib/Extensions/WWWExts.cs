using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class WWWExts {
    public static Either<WWWError, WWW> toEither(this WWW www) {
      return 
        string.IsNullOrEmpty(www.error)
        ? F.right<WWWError, WWW>(www)
        : F.left<WWWError, WWW>(new WWWError(www));
    }
  }
}
