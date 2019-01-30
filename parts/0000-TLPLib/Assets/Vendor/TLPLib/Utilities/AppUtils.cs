namespace com.tinylabproductions.TLPLib.Utilities {
  public enum AppTargetPlatform {
    Windows, OSX, Linux, Android, iOS
  }

  public static class AppUtils {
    /** Useful for determining in editor runtime which target platform we're running in. **/
    public static AppTargetPlatform targetPlatform =>
#if UNITY_ANDROID
      AppTargetPlatform.Android
#elif UNITY_IOS
      AppTargetPlatform.iOS
#elif UNITY_STANDALONE_WIN
      AppTargetPlatform.Windows
#elif UNITY_STANDALONE_OSX
      AppTargetPlatform.OSX
#elif UNITY_STANDALONE_LINUX
      AppTargetPlatform.Linux
#else
      // define a new target here.
      ???
#endif
      ;
  }
}
