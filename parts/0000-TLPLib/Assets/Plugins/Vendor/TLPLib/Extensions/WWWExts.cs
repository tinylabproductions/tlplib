using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class WWWExts {
    public static Either<WWWError, WWW> toEither(this WWW www) => 
      string.IsNullOrEmpty(www.error)
      ? F.right<WWWError, WWW>(www)
      : F.left<WWWError, WWW>(new WWWError(www));

    public static Either<WWWError, Texture2D> asTexture(this Either<WWWError, WWW> either) =>
      either.flatMapRight(www =>
        !www.texture
          ? Either<WWWError, Texture2D>.Left(new WWWError(www, "WWW didn't produce a texture!"))
          : Either<WWWError, Texture2D>.Right(www.texture)
      );
  }
}
