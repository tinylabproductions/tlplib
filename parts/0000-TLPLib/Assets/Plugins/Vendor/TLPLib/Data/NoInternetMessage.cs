using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  public partial struct NoInternetMessage {
    [PublicAccessor] readonly string _message;
  }

  [Record]
  public partial struct WebRequestError {
    [PublicAccessor] readonly Either<LogEntry, NoInternetMessage> _message;

    public static WebRequestError noInternet(NoInternetMessage nim) =>
      new WebRequestError(new Either<LogEntry, NoInternetMessage>(nim));

    public static WebRequestError logEntry(LogEntry errorMsg) =>
      new WebRequestError(new Either<LogEntry, NoInternetMessage>(errorMsg));
  }

}
