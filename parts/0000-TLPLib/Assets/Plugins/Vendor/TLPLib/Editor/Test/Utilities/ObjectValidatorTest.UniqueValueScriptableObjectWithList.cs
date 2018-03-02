using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Utilities.Editor;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class UniqueValueScriptableObjectWithList : ScriptableObject {
    public List<ObjectValidatorTest.UniqueValueStruct> listOfStructs;

    public void  set(List<ObjectValidatorTest.UniqueValueStruct> val) {
      listOfStructs = val;
    }
  }
}