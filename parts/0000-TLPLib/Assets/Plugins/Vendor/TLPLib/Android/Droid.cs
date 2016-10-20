using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.reflection;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
#if UNITY_ANDROID
  /* DSL for nicer android object instantiation. */
  public static class Droid {
    #region class names

    public const string CN_CONTEXT = "android.content.Context";
    public const string CN_INTENT = "android.content.Intent";
    public const string CN_SERVICE_CONNECTION = "android.content.ServiceConnection";
    public const string CN_ILICENSING_SERVICE = "com.android.vending.licensing.ILicensingService";
    public const string CN_ILICENSING_SERVICE_STUB = "com.google.android.vending.licensing.ILicensingService$Stub";
    public const string CN_ILICENSE_RESULT_LISTENER = "com.google.android.vending.licensing.ILicenseResultListener";

    #endregion

    #region static methods

    /* New java object. */
    public static AndroidJavaObject jo(string className, params object[] args) 
    { return new AndroidJavaObject(className, args); }

    /* New java class. */
    public static AndroidJavaClass jc(string className) 
    { return new AndroidJavaClass(className); }

    /* New intent. */
    public static AndroidJavaObject intent(params object[] args)
    { return jo(CN_INTENT, args); }

    public static bool hasSystemFeature(string feature) => 
      AndroidActivity.packageManager.hasSystemFeature(feature);

    private static readonly Lazy<bool> _hasTouchscreen = 
      F.lazy(() => hasSystemFeature("android.hardware.touchscreen"));

    /* Is touchscreen supported? */
    public static bool hasTouchscreen { get { return _hasTouchscreen.get; } }

    #endregion

    #region extension methods

    /* New java class: string extension method. */
    public static AndroidJavaClass javaClass(this string className) { return jc(className); }

    /* New java object: string extension method. */
    public static AndroidJavaObject javaObject(
      this string className, params object[] args
    ) { return jo(className, args); }

    /* Extension method: call instance method on java object and return other 
     * java object. */
    public static AndroidJavaObject cjo(
      this AndroidJavaObject javaObject, string methodName, params object[] args
    ) { return javaObject.Call<AndroidJavaObject>(methodName, args); }

    /* Extension method: call static method on java object and return other 
     * java object. */
    public static AndroidJavaObject csjo(
      this AndroidJavaObject javaObject, string methodName, params object[] args
    ) { return javaObject.CallStatic<AndroidJavaObject>(methodName, args); }

    /* Extension method: call instance method on java object. */
    public static A c<A>(
      this AndroidJavaObject javaObject, string methodName, params object[] args
    ) { return javaObject.Call<A>(methodName, args); }

    /** Access to ```internal AndroidJavaObject(IntPtr jobject)``` */
    static readonly Fn<object[], AndroidJavaObject> ajoCreator = 
      PrivateConstructor.creator<AndroidJavaObject>();

    /** 
     * Unity AndroidJavaObject throws an exception if a call from Java returns null
     * so we have our own implementation.
     */
    public static AndroidJavaObject cjoReturningNull(
      this AndroidJavaObject javaObject, string methodName, params object[] args
    ) {
      if (args == null) args = new object[1];
      var methodId = AndroidJNIHelper.GetMethodID<AndroidJavaObject>(
        javaObject.GetRawClass(), methodName, args, false
      );
      var jniArgArray = AndroidJNIHelper.CreateJNIArgArray(args);
      try {
        var returned = AndroidJNI.CallObjectMethod(
          javaObject.GetRawObject(), methodId, jniArgArray
        );
        if (returned == IntPtr.Zero) return null;
        else {
          try { return ajoCreator(new object[] {returned}); }
          finally { AndroidJNI.DeleteLocalRef(returned); }
        }
      }
      finally {
        AndroidJNIHelper.DeleteJNIArgArray(args, jniArgArray);
      }
    }

    #endregion
  }
#endif
}
