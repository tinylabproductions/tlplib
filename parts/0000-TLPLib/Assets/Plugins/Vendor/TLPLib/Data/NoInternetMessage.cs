using GenerationAttributes;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  public partial struct NoInternetMessage {
    [PublicAccessor] readonly string _message;
  }
}
