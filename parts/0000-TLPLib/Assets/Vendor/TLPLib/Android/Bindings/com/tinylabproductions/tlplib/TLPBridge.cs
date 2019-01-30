#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.telephony;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib {
  public class TLPBridge {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("com.tinylabproductions.tlplib.Bridge");

    public static Future<Either<string, Option<string>>> countryCodeFromLastKnownLocation { get {
      return Future<Either<string, Option<string>>>.async(p => new JThread(() => {
        Either<string, Option<string>> ret;
        try {
          ret = Either<string, Option<string>>.Right(
            TelephonyManager.jStringToCountryCode(
              klass.CallStatic<string>("countryCodeFromLastKnownLocation")
            )
          );
        }
        catch (AndroidJavaException e) {
          ret = Either<string, Option<string>>.Left(
            $"Error in {nameof(TLPBridge)}.{nameof(countryCodeFromLastKnownLocation)}: {e}"
          );
        }
        ASync.OnMainThread(() => p.complete(ret));
      }).start());
    } }
  }
}
#endif