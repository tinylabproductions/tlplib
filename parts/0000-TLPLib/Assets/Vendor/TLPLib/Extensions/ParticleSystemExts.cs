using UnityEngine;

namespace Assets.Vendor.TLPLib.Extensions {
  public static class ParticleSystemExts {
    public static void setEmmissionEnabled(this ParticleSystem particleSystem, bool enabled) {
      var emmission = particleSystem.emission;
      emmission.enabled = enabled;
    }
  }
}
