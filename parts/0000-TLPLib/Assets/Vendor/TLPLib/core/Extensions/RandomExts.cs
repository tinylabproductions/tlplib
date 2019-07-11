using System;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class RandomExts {

    public static float nextFloat(this Random random, float upperBound) { return nextFloat(random, 0, upperBound); }

    public static float nextFloat(this Random random, float lowerBound, float upperBound) {
      return (float) (lowerBound + random.NextDouble()*(upperBound - lowerBound));
    }


  }
}