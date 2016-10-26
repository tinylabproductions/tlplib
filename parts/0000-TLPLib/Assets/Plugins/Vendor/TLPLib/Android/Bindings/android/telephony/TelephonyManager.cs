#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.telephony {
  public enum PhoneType {
    NONE = 0, GSM = 1, CDMA = 2, SIP = 3
  }

  public class TelephonyManager : Binding {
    public TelephonyManager(AndroidJavaObject java) : base(java) {}

    public static Option<string> jStringToCountryCode(string jString) =>
      F.opt(jString).filter(s => s.Length == 2).map(s => s.ToLower());

    Option<string> countryIso(string method) {
      try {
        return jStringToCountryCode(java.Call<string>(method));
      }
      catch (AndroidJavaException e) {
        if (Log.isDebug) Log.rdebug($"Error fetching country iso code from android via '{method}': {e}");
        return F.none<string>();
      }
    }

    public Option<string> simCountryIso => countryIso("getSimCountryIso");
    /** Result may be unreliable on CDMA networks. */
    public Option<string> networkCountryIso => countryIso("getNetworkCountryIso");
    public PhoneType phoneType => (PhoneType) java.c<int>("getPhoneType");

    public Option<string> phoneCountryIso =>
      simCountryIso 
      || (phoneType == PhoneType.CDMA ? F.none<string>() : networkCountryIso);
  }
}

#endif
      