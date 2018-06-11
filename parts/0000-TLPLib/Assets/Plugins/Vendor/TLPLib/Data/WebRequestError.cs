using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  public partial struct WebRequestError {
    [PublicAPI] public readonly Url url;
    [PublicAPI] public readonly Either<ErrorMsg, NoInternetError> message;

    [PublicAPI] public ErrorMsg simplify => message.fold(
      err => err, 
      nie => new ErrorMsg($"No internet: ${nie.message}", reportToErrorTracking: false)
    );
  }
  
  [Record]
  public partial struct NoInternetError {
    [PublicAPI] public readonly string message;
  }
}
