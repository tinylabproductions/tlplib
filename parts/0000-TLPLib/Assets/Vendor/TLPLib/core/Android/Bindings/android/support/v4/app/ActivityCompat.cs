
#if UNITY_ANDROID
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Android.Bindings.android.app;
using com.tinylabproductions.TLPLib.Android.Bindings.android.support.v4.content;
using com.tinylabproductions.TLPLib.Data;
using pzd.lib.serialization;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.support.v4.app {
  public class ActivityCompat : ContextCompat {
    static readonly AndroidJavaClass klass =
      new AndroidJavaClass("android.support.v4.app.ActivityCompat");

    static readonly PrefVal<ImmutableHashSet<string>> permissionsRequestedBefore =
      PrefVal.player.hashSet(
        "TLPLib_Android_ActivityCompat_PermissionsRequestedBefore",
        SerializedRW.str
      );

    /// <summary>
    /// This method returns true if the app has requested this permission previously
    /// and the user denied the request.
    ///
    /// Note: If the user turned down the permission request in the past and chose
    /// the Don't ask again option in the permission request system dialog,
    /// this method returns false. The method also returns false if a device policy
    /// prohibits the app from having that permission.
    /// </summary>
    public static bool shouldShowRequestPermissionRationale(
      string permission, Activity activity = null
    ) => klass.CallStatic<bool>(
      "shouldShowRequestPermissionRationale",
      (activity ?? AndroidActivity.current).java, permission
    );

    /// <summary>
    /// Same as <see cref="shouldShowRequestPermissionRationale"/>, but checks with a
    /// local <see cref="PrefVal"/> to check if this is the first time we ask for this
    /// permission.
    /// </summary>
    public static bool shouldShowRequestPermissionRationaleIncludingFirstTime(
      string permission, Activity activity = null
    ) =>
      !permissionsRequestedBefore.value.Contains(permission)
      || shouldShowRequestPermissionRationale(permission, activity);

    public static void requestPermissions(
      string[] permissions,
      Activity activity = null,
      int requestCode = 0
    ) {
      klass.CallStatic(
        "requestPermissions",
        (activity ?? AndroidActivity.current).java, permissions, requestCode
      );

      // We should only record changes if call to android does not throw an exception
      var askedBefore = permissionsRequestedBefore.value;
      foreach (var permission in permissions) {
        if (!askedBefore.Contains(permission)) {
          askedBefore = askedBefore.Add(permission);
        }
      }
      permissionsRequestedBefore.value = askedBefore;
    }
  }
}
#endif