#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class AndroidExts {
    public static string asString(this AndroidJavaObject obj) {
      using (var stringObj = obj.Call<AndroidJavaObject>("toString")) {
        using (
          var bytesObj = stringObj.Call<AndroidJavaObject>("getBytes", "UTF8")
        ) {
          var bytes = bytesObj.asBytes();
          return bytes == null 
            ? null : System.Text.Encoding.UTF8.GetString(bytes);
        }
      }
    }

    public static byte[] asBytes(this AndroidJavaObject obj) {
      var raw = obj.GetRawObject();
      return raw.ToInt32() == 0 
        ? null : AndroidJNIHelper.ConvertFromJNIArray<byte[]>(raw);
    }
  }
}
#endif
