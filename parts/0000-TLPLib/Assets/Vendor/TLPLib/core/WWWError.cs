using UnityEngine;

// obsolete WWW
#pragma warning disable 618

namespace com.tinylabproductions.TLPLib {
  /** Error that has a www context. */
  public struct WWWError {
    public readonly WWW www;
    public readonly string error;

    public WWWError(WWW www) : this(www, www.error) {}

    public WWWError(WWW www, string error) {
      this.www = www;
      this.error = error;
    }

    public override string ToString() => $"{nameof(WWWError)}[{error}]";
  }
}
