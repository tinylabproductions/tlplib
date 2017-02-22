using System;

namespace com.tinylabproductions.TLPLib.Android.Ads {
  public interface IStandardIncentivized : IStandardInterstitial {
    event Action<bool> adWatched;
  }
}