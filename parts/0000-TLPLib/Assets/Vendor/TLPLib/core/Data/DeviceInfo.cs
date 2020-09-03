#if UNITY_ANDROID
#endif
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  public partial struct DeviceInfo {
    public readonly string manufacturer, modelCode;

    public static Option<DeviceInfo> create() {
#if UNITY_ANDROID && !UNITY_EDITOR
      return F.some(new DeviceInfo(manufacturer: Build.MANUFACTURER, modelCode: Build.DEVICE));
#endif
      return F.none<DeviceInfo>();
    }
  }
}