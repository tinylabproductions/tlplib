using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ParticleSystemExts {
    public static void setEmissionEnabled(this ParticleSystem particleSystem, bool enabled) {
      var emmission = particleSystem.emission;
      emmission.enabled = enabled;
    }
  }
}
