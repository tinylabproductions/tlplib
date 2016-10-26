#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Android.Bindings.android.os;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.firebase.analytics {
  public class FirebaseAnalytics : Binding, IFirebaseAnalytics {
    public static IFirebaseAnalytics instance;

    static FirebaseAnalytics() {
      if (Application.platform == RuntimePlatform.Android)
        using (var klass = new AndroidJavaClass("com.google.firebase.analytics.FirebaseAnalytics"))
          instance = new FirebaseAnalytics(klass.csjo("getInstance", AndroidActivity.current.java));
      else {
        instance = new FirebaseAnalyticsNoOp();
      }
    }

    FirebaseAnalytics(AndroidJavaObject java) : base(java) { }

    public void logEvent(FirebaseEvent data) {
      // Passing null indicates that the event has no parameters.
      var parameterBundle = data.parameters.isEmpty()
        ? null : fillParameterBundle(data.parameters).java;

      java.Call("logEvent", data.name, parameterBundle);
    }

    Bundle fillParameterBundle(
      IEnumerable<KeyValuePair<string, OneOf<string, long, double>>> parameters
    ) {
      var parameterBundle = new Bundle();
      foreach (var kv in parameters) {
        var val = kv.Value;
        switch (val.whichOne) {
          case OneOf.Choice.A:
            parameterBundle.putString(kv.Key, val.__unsafeGetA);
            break;
          case OneOf.Choice.B:
            parameterBundle.putLong(kv.Key, val.__unsafeGetB);
            break;
          case OneOf.Choice.C:
            parameterBundle.putDouble(kv.Key, val.__unsafeGetC);
            break;
          default:
            throw new ArgumentOutOfRangeException(
              nameof(val.whichOne), val.whichOne, "Unknown which one."
            );
        }
      }
      return parameterBundle;
    }

    public void setMinimumSessionDuration(Duration duration) {
      java.Call("setMinimumSessionDuration", (long) duration.millis);
    }

    public void setSessionTimeoutDuration(Duration duration) {
      java.Call("setSessionTimeoutDuration", (long) duration.millis);
    }

    public void setUserId(FirebaseUserId id) {
      java.Call("setUserId", id.id);
    }
  }
}
#endif