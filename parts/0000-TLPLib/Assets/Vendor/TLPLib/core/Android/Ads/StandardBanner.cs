﻿using GenerationAttributes;
using pzd.lib.log;
using pzd.lib.reactive;

#if UNITY_ANDROID
using System.Collections.Immutable;
using System.Linq;
using pzd.lib.exts;
using UnityEngine;
using pzd.lib.dispose;
using pzd.lib.functional;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Logger;

#endif

namespace com.tinylabproductions.TLPLib.Android.Ads {
  public interface IStandardBanner {
    void setVisibility(bool visible);
    void load();
    void destroy();
    void onPause();
    void onResume();
  }

  public interface IStandardBannerKnowsState : IStandardBanner {
    IRxVal<bool> hasAd { get; }
  }

#if UNITY_ANDROID
  // Banners that extend com.tinylabproductions.tlplib.ads.BannerBase in Java.
  public class StandardBanner : IStandardBanner {
    [LazyProperty, Implicit] static ILog log => Log.d.withScope(nameof(StandardBanner));

    protected readonly AndroidJavaObject java;
    readonly DisposableTracker dt = new DisposableTracker();

    protected StandardBanner(AndroidJavaObject java) {
      this.java = java;
      ASync.onAppPause.subscribe(dt, paused => {
        if (paused) onPause();
        else onResume();
      });
      ASync.onAppQuit.subscribe(dt, _ => destroy());
    }

    public void setVisibility(bool visible) => java.Call("setVisibility", visible);
    public void load() => java.Call("load");
    public void destroy() {
      dt.Dispose();
      java.Call("destroy");
    }
    public void onPause() => java.Call("onPause");
    public void onResume() => java.Call("onResume");
  }

  public class StandardBannerAggregator : IStandardBannerKnowsState {
    readonly ImmutableList<IStandardBannerKnowsState> banners;
    public IRxVal<bool> hasAd { get; }

    Option<IStandardBannerKnowsState> active = None._;

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
        active = None._;
      }
    }

    public void setVisibility(bool visible) {
      if (visible) play();
      else close();
    }

    public void load() { foreach (var b in banners) b.load(); }

    public void destroy() { foreach (var b in banners) b.destroy(); }

    public void onPause() { foreach (var b in banners) b.onPause(); }

    public void onResume() { foreach (var b in banners) b.onResume(); }
  }
#endif
}
