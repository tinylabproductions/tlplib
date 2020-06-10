using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public interface OnObjectValidate {
    /// <summary>
    /// Is validation of this object thread safe? If false is returned the validation will be scheduled to be executed
    /// on Unity main thread. 
    /// </summary>
    bool onObjectValidateIsThreadSafe { get; }
    /// <summary>
    /// onObjectValidate is called when ObjectValidator
    /// begins to validate the object implementing this interface.
    /// <param name="containingComponent">
    /// Can be used, for example, to mark any field updates
    /// during build time using .recordEditorChanges
    /// </param>
    /// </summary>
    IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent);
  }
}
