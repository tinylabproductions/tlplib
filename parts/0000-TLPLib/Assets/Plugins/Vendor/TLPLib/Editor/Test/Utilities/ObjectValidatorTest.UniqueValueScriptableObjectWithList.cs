using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Utilities.Editor;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  [Record]
  public partial class UniqueValueScriptableObjectWithList : ScriptableObject {
    public List<ObjectValidatorTest.UniqueValueStruct> listOfStructs;
  }
}