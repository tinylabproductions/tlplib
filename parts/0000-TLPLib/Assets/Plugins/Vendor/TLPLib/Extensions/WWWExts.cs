using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class WWWExts {
    public static Either<WWWError, WWW> toEither(this WWW www) => 
      string.IsNullOrEmpty(www.error)
      ? F.right<WWWError, WWW>(www)
      : F.left<WWWError, WWW>(new WWWError(www));

    public static Either<WWWError, Texture2D> asTexture(this Either<WWWError, WWW> either) =>
      either.flatMapRight(www => {
        // NonReadable textures take 2x less ram
        var tex = www.textureNonReadable;
        return tex
          ? Either<WWWError, Texture2D>.Right(tex)
          : Either<WWWError, Texture2D>.Left(new WWWError(www, "WWW didn't produce a texture!"));
      });

    public static WWWWithHeaders headers(this WWW www) => 
      new WWWWithHeaders(www, www.responseHeaders.asReadOnly());
  }

  /** 
   * Struct that wraps the parsed headers, because `www.responseHeaders` parses headers 
   * each time it is called.
   **/
  public struct WWWWithHeaders {
    public readonly WWW www;
    public readonly IReadOnlyDictionary<string, string> headers;

    public WWWWithHeaders(WWW www, IReadOnlyDictionary<string, string> headers) {
      this.www = www;
      this.headers = headers;
    }

    public Option<string> this[string key] => headers.get(key);
  }
}
