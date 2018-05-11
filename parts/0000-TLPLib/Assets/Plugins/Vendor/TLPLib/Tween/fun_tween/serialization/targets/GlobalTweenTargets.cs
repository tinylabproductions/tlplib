using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.targets {
  [CreateAssetMenu(menuName = "TLPLib/Global Tween Targets")]
  public class GlobalTweenTargets : ScriptableObject {
    [PublicAPI] public static readonly TweenMutator<Color, GlobalTweenTargets>
      globalFogColor = (value, _, relative) => TweenMutators.globalFogColor(value, F.unit, relative);
    
    [PublicAPI] public static readonly TweenMutator<float, GlobalTweenTargets>
      globalFogDensity = (value, _, relative) => TweenMutators.globalFogDensity(value, F.unit, relative);
  }
}