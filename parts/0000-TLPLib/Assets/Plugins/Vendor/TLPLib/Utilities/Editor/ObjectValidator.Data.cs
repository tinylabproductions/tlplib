using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    /// <param name="containingObject">Unity object </param>
    /// <param name="obj">Object that is being validated.</param>
    /// <returns></returns>
    public delegate IEnumerable<ErrorMsg> CustomObjectValidator(Object containingObject, object obj);

    [Record]
    public partial struct Progress {
      public readonly int currentIdx, total;

      public float ratio => (float) currentIdx / total;
    }
  }
}