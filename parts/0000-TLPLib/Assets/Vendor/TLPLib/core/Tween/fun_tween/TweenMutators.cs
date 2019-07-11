namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public delegate void TweenMutator<in PropertyType, in TargetType>(
    PropertyType value, TargetType target, bool isRelative
  );
  public static class TweenMutators {}
}