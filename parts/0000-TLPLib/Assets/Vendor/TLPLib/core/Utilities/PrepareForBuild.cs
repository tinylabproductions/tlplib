using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public interface OnObjectValidate {
    /// <summary>
    /// onObjectValidate is called when ObjectValidator
    /// begins to validate the object implementing this interface.
    /// <param name="containingComponent">
    /// Can be used, for example, to mark any field updates
    /// during build time using .recordEditorChanges
    /// </param>
    /// </summary>
    IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent);
    bool onObjectValidateIsThreadSafe { get; }
  }
}
