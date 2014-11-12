using System;

namespace com.tinylabproductions.TLPLib {
  /**
   * Illegal state exception.
   * 
   * http://www.youtube.com/watch?v=WIXGUzRo3H0
   **/
  public class IllegalStateException : Exception {
    public IllegalStateException() : base("How did I get here?") {}
    public IllegalStateException(string message) : base(message) {}
  }
}
