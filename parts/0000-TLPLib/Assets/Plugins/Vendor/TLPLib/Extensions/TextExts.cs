using UnityEngine.UI;

namespace Plugins.Vendor.TLPLib.Extensions {
  public static class TextExts {
    public static void setText(this Text text, uint s) => text.setText(s.ToString());
    public static void setText(this Text text, string s) => text.text = s;
  }
}