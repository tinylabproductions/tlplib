using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  public partial struct WebRequestError {
    public readonly Url url;
    public readonly Either<ErrorMsg, NoInternetError> message;

    public ErrorMsg simplify => message.fold(err => err, nie => new ErrorMsg($"No internet: ${nie.message}"));
  }
  
  [Record]
  public partial struct NoInternetError {
    public readonly string message;
  }
}
