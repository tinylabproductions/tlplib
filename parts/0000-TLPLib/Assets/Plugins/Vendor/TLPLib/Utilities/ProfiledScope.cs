using System;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
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
    readonly string name;
    
    public ProfiledScope(string name) {
      this.name = name;
      if (Log.d.isDebug()) Log.d.debug($"{nameof(ProfiledScope)} begin @ frame {Time.frameCount}: {name}");
      Profiler.BeginSample(name);
    }

    public void Dispose() {
      if (Log.d.isDebug()) Log.d.debug($"{nameof(ProfiledScope)} end @ frame {Time.frameCount}: {name}");
      Profiler.EndSample();
    }
  }
}