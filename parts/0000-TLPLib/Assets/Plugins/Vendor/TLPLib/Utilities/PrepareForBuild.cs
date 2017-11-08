namespace com.tinylabproductions.TLPLib.Utilities {
  /// <summary>
  /// onObjectValidate is called whenever object validator runs any checks 
  /// on the object implementing OnObjectValidate
  /// </summary>
  public interface OnObjectValidate {
    void onObjectValidate(UnityEngine.Object containingComponent);
  }
}