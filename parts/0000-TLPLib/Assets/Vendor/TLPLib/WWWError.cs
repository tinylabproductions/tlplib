using UnityEngine;

namespace com.tinylabproductions.TLPLib {
  public struct WWWError {
    public readonly WWW www;

    public WWWError(WWW www) { this.www = www; }
    public string error => www.error;
  }
}
