using com.tinylabproductions.TLPLib.validations;
using UnityEngine;

namespace Plugins.Vendor.TLPLib.Editor.Test.Utilities {
  public class UniqueValueScriptableObject : ScriptableObject {
    [UniqueValue("category")] public byte[] identifier;
    [UniqueValue("category")] public byte[] identifier2;

    public void set(byte[] a, byte[] b) {
      identifier = a;
      identifier2 = b;
    }
  }
}