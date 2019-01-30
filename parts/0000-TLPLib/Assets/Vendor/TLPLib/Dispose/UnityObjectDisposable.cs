using System;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Dispose {
  public class UnityObjectDisposable : IDisposable {
    public readonly Object obj;

    public UnityObjectDisposable(Object obj) { this.obj = obj; }

    public void Dispose() => Object.Destroy(obj);
  }

  public static class UnityObjectDisposableExts {
    public static UnityObjectDisposable asDisposable(this Object obj) =>
      new UnityObjectDisposable(obj);
  }
}