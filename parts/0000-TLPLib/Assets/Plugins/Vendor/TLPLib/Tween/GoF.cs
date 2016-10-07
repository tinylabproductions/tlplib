#if GOTWEEN
using System;

namespace com.tinylabproductions.TLPLib.Tween {
  /** 
   * Facade for GoKit <https://github.com/prime31/GoKit> library 
   * to allow type safe usage of it
   **/
  public static class GoF {
    /**
     * Don't forget to use `TF.Prop` as a property name for GoTweenConfig 
     * options.
     **/
    public static GoTween to<A>(
      Fn<A> getter, Act<A> setter, float duration,
      Fn<GoTweenConfig, GoTweenConfig> config
    ) {
      return Go.to(TF.a(getter, setter), duration, config(new GoTweenConfig()));
    }
  }
}
#endif