using com.tinylabproductions.TLPLib.Android.Bindings.java.io;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.os {
  public static class Environment {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("android.os.Environment");

    static string DIRECTORY_DOWNLOADS => klass.GetStatic<string>("DIRECTORY_DOWNLOADS");

    public static File getExternalStoragePublicDirectoryDownloads() =>
      getExternalStoragePublicDirectory(DIRECTORY_DOWNLOADS);

    public static File getExternalStoragePublicDirectory(string type) =>
      new File(klass.csjo("getExternalStoragePublicDirectory", type));
  }
}