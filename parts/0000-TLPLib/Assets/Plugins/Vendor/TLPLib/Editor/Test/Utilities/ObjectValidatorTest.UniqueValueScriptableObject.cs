using com.tinylabproductions.TLPLib.validations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class UniqueValueScriptableObject : ScriptableObject {
    [UniqueValue("category")] public byte[] identifier;
    [UniqueValue("category")] public byte[] identifier2;

    public void set(byte[] a, byte[] b) {
      identifier = a;
      identifier2 = b;
    }
  }
}