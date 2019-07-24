﻿using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  public partial class WebRequestError {
    [PublicAPI] public readonly Url url;
    [PublicAPI] public readonly Either<LogEntry, NoInternetError> message;

    [PublicAPI] public LogEntry simplify => message.fold(
      err => err, 
      nie => new ErrorMsg($"No internet: {nie.message}", reportToErrorTracking: false)
    );
  }
  
  [Record]
  public partial class NoInternetError {
    [PublicAPI] public readonly string message;
  }
}
