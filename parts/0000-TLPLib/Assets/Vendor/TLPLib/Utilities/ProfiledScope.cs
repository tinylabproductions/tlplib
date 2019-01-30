using System;
using UnityEngine.Profiling;

namespace com.tinylabproductions.TLPLib.Utilities {
  /// <summary>
  /// Allows using profiles safely and without garbage creation.
  ///
  /// <code><![CDATA[
  /// using (var _ = new ProfiledScope("scope name")) {
  ///   // your code here
  /// }
  /// ]]></code>
  /// </summary>
  public struct ProfiledScope : IDisposable {
    public ProfiledScope(string name) {
      Profiler.BeginSample(name);
    }

    public void Dispose() {
      Profiler.EndSample();
    }
  }
}