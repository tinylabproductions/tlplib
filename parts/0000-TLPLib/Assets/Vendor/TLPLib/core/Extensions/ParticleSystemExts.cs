using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ParticleSystemExts {
    [PublicAPI]
    public static void restart(this ParticleSystem ps) {
      ps.time = 0;
      ps.Play();
    }
    
    [PublicAPI]
    public static void setEmissionEnabled(this ParticleSystem particleSystem, bool enabled) {
      var emmission = particleSystem.emission;
      emmission.enabled = enabled;
    }

    [PublicAPI]
    public static void playWithoutChildren(this ParticleSystem[] array) {
      foreach (var ps in array) {
        ps.Play(withChildren: false);
      }
    }

    [PublicAPI]
    public static void stopWithoutChildren(this ParticleSystem[] array, ParticleSystemStopBehavior stopBehavior) {
      foreach (var ps in array) {
        ps.Stop(withChildren: false, stopBehavior);
      }
    }
  }
}
