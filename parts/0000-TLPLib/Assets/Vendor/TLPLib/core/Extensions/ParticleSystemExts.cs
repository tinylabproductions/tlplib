using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI]
  public static class ParticleSystemExts {
    public static void restart(this ParticleSystem ps) {
      ps.time = 0;
      ps.Play();
    }
    
    public static void setEmissionEnabled(this ParticleSystem particleSystem, bool enabled) {
      var emmission = particleSystem.emission;
      emmission.enabled = enabled;
    }

    public static void playWithoutChildren(this ParticleSystem[] array) {
      foreach (var ps in array) {
        ps.Play(withChildren: false);
      }
    }

    public static void stopWithoutChildren(this ParticleSystem[] array, ParticleSystemStopBehavior stopBehavior) {
      foreach (var ps in array) {
        ps.Stop(withChildren: false, stopBehavior);
      }
    }
    
    public static void setColorOverLifeTime(this ParticleSystem system, ParticleSystem.MinMaxGradient gradient) {
      var colorOverLifeTime = system.colorOverLifetime;
      colorOverLifeTime.color = gradient;
    }
    
    public static void setStartColor(this ParticleSystem system, Color color) {
      var main = system.main;
      main.startColor = color;
    }
  }
}
