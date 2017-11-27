#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm {
  public class PermissionInfo : Binding {
    // ReSharper disable once EnumUnderlyingTypeIsInt
    public enum ProtectionLevel : int {
      // https://developer.android.com/reference/android/content/pm/PermissionInfo.html#PROTECTION_NORMAL
      Normal = 0,
      // https://developer.android.com/reference/android/content/pm/PermissionInfo.html#PROTECTION_DANGEROUS
      Dangerous = 1,
      // https://developer.android.com/reference/android/content/pm/PermissionInfo.html#PROTECTION_SIGNATURE
      Signature = 2,
      // https://developer.android.com/reference/android/content/pm/PermissionInfo.html#PROTECTION_FLAG_SYSTEM
      FlagSystem = 16,
      // https://developer.android.com/reference/android/content/pm/PermissionInfo.html#PROTECTION_FLAG_DEVELOPMENT
      FlagDevelopment = 32
    }

    public readonly string name;
    readonly ProtectionLevel level;

    public PermissionInfo(string name, AndroidJavaObject java) : base(java) {
      this.name = name;
      level = (ProtectionLevel) java.Get<int>("protectionLevel");
    }

    public bool isProtectionLevel(ProtectionLevel l) =>
      // Checks with regards to bitmasks.
      ((int) level & (int) l) == (int) l;
  }
}
#endif