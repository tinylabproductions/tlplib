using System;

namespace com.tinylabproductions.TLPLib.Android.Ads {
  public interface IStandardRewarded : IStandardInterstitial {
    event Action<bool> adWatched;
  }
}