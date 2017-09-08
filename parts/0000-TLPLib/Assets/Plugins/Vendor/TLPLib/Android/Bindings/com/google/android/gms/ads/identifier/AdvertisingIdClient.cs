﻿using System;
#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
#endif
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.android.gms.ads.identifier {
  public interface AdvertisingIdClientInfo {
    string id { get; }
    bool limitAdTrackingEnabled { get; }
  }

  public interface IAdvertisingIdClient {
    Try<AdvertisingIdClientInfo> getAdvertisingIdInfoForCurrentActivity();
#if UNITY_ANDROID
    Try<AdvertisingIdClientInfo> getAdvertisingIdInfo(Context context);
#endif
  }

  public static class AdvertisingIdClient {
    public static readonly Option<IAdvertisingIdClient> instance =
#if UNITY_ANDROID
      Application.platform == RuntimePlatform.Android
        ? new AdvertisingIdClientAndroid().some<IAdvertisingIdClient>()
        : Option<IAdvertisingIdClient>.None
#else
      Option<IAdvertisingIdClient>.None
#endif
      ;
  }

#if UNITY_ANDROID
  class AdvertisingIdClientAndroid : IAdvertisingIdClient {
    /** Includes both the advertising ID as well as the limit ad tracking setting. */
    public class Info : AdvertisingIdClientInfo {
      public string id { get; }
      public bool limitAdTrackingEnabled { get; }

      public Info(AndroidJavaObject java) {
        id = java.Call<string>("getId");
        limitAdTrackingEnabled = java.c<bool>("isLimitAdTrackingEnabled");
      }

      public override string ToString() => 
        $"{nameof(AdvertisingIdClientInfo)}[" +
        $"{nameof(id)}: {id}, " +
        $"{nameof(limitAdTrackingEnabled)}: {limitAdTrackingEnabled}" +
        $"]";
    }

    static A withKlass<A>(Fn<AndroidJavaClass, A> f) {
      using (var klass = new AndroidJavaClass(
        "com.google.android.gms.ads.identifier.AdvertisingIdClient"
      )) return f(klass);
    }

    public Try<AdvertisingIdClientInfo> getAdvertisingIdInfoForCurrentActivity() =>
      getAdvertisingIdInfo(AndroidActivity.current);

    public Try<AdvertisingIdClientInfo> getAdvertisingIdInfo(Context context) => 
      F.doTry(() => withKlass(klass => 
        (AdvertisingIdClientInfo) new Info(klass.csjo("getAdvertisingIdInfo", context.java))
      ));
  }
#endif
}