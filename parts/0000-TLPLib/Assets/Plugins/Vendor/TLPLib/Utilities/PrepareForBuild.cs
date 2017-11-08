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
    void onObjectValidate(UnityEngine.Object containingComponent);
  }
}
