using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Profiling {
  public static class Profile {
    public static A block<A>(string name, Fn<A> f) {
      Profiler.BeginSample(name);
      var a = f();
      Profiler.EndSample();
      return a;
    }

    public static void block(string name, Act f) {
      Profiler.BeginSample(name);
      f();
      Profiler.EndSample();
    }
  }
}
