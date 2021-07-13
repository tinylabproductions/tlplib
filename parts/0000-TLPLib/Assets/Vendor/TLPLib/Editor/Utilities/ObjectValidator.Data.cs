using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using GenerationAttributes;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    public interface CustomObjectValidator {
      bool isThreadSafe { get; }

      /// <param name="containingObject">Unity object </param>
      /// <param name="obj">Object that is being validated.</param>
      /// <param name="field">Field being validated.</param>
      IEnumerable<ErrorMsg> validateField(Object containingObject, object obj, StructureCache.Field field);
      
      IEnumerable<ErrorMsg> validateComponent(Object component);
    }

    [Record] public partial struct Progress {
      public readonly int currentIdx, total;
      public readonly Func<string> customText;

      public float ratio => (float) currentIdx / total;
      
      public string text {
        get {
          var custom = customText();
          var txt = $"{currentIdx} / {total}";
          return string.IsNullOrEmpty(custom) ? txt : $"{custom} ({txt})";
        }
      }
    }
  }
}