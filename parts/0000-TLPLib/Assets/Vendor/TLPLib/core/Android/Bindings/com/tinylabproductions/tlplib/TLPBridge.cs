#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.telephony;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib {
  [PublicAPI] public static class TLPBridge {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("com.tinylabproductions.tlplib.Bridge");

    static Option<bool> _isTablet = F.none<bool>();

    public static bool isTablet { get {
      if (_isTablet.isNone) {
        // cache result
        _isTablet = F.some(klass.CallStatic<bool>("isTablet"));
      }
      return _isTablet.get;
    } }

    public static void sharePNG(string path, string title, string sharerText) {
      klass.CallStatic("sharePNG", path, title, sharerText);
    }
    
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