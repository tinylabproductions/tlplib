using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Ads {
  public interface IStandardBanner {
    void setVisibility(bool visible);
    void load();
    void destroy();
  }

  public interface IStandardBannerKnowsState : IStandardBanner {
    IRxVal<bool> hasAd { get; }
  }

#if UNITY_ANDROID
  // Banners that extend com.tinylabproductions.tlplib.ads.BannerBase in Java.
  public class StandardBanner : IStandardBanner {
    protected readonly AndroidJavaObject java;

    public StandardBanner(AndroidJavaObject java) { this.java = java; }

    public void setVisibility(bool visible) => java.Call("setVisibility", visible);
    public void load() => java.Call("load");
    public void destroy() => java.Call("destroy");
  }

  public class StandardBannerAggregator : IStandardBannerKnowsState {
    readonly ImmutableList<IStandardBannerKnowsState> banners;
    public IRxVal<bool> hasAd { get; }

    Option<IStandardBannerKnowsState> active = Option<IStandardBannerKnowsState>.None;

    public StandardBannerAggregator(
      ImmutableList<IStandardBannerKnowsState> banners
    ) {
      this.banners = banners;
      hasAd = banners.Select(_ => _.hasAd).anyOf();
    }

    public bool play() {
      foreach (var b in banners) {
        if (b.hasAd.value) {
          active = b.some();
          b.setVisibility(true);
          return true;
        }
      }
      return false;
    }

    public void close() {
      foreach (var b in active) {
        b.setVisibility(false);
        active = Option<IStandardBannerKnowsState>.None;
      }
    }

    public void setVisibility(bool visible) {
      if (visible) play();
      else close();
    }

    public void load() { foreach (var b in banners) b.load(); }

    public void destroy() { foreach (var b in banners) b.destroy(); }
  }
#endif
}
