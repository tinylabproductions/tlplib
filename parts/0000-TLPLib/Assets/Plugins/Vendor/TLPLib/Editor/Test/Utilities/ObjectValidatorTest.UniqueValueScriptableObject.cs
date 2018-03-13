using com.tinylabproductions.TLPLib.validations;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  [Record]
  public partial class UniqueValueScriptableObject : ScriptableObject {
    [UniqueValue(ObjectValidatorTest.UNIQUE_CATEGORY)] public byte[] identifier;
    [UniqueValue(ObjectValidatorTest.UNIQUE_CATEGORY)] public byte[] identifier2;
  }
}