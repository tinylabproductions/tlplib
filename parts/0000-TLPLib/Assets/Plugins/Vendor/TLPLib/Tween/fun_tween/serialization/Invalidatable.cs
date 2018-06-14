namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization {
  public interface Invalidatable {
    /// <summary>
    /// Force recreation of tween structure. Useful when you need to see changes made in editor.
    /// </summary>
    void invalidate();
  }
}