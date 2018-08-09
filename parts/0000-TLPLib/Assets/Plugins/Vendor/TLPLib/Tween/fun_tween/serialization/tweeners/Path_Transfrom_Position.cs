using com.tinylabproductions.TLPLib.Tween.fun_tween.path;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.tweeners {
  [AddComponentMenu("")]
  public class Path_Transfrom_Position : SerializedTweener<float, float, Transform> {
#pragma warning disable 649
    [SerializeField, NotNull] Vector3PathBehaviour pathBehaviour;
#pragma warning restore 649
    public Path_Transfrom_Position() : base(
      TweenOps.float_, SerializedTweenerOps.Add.float_,
      // Paths do not have current state, so their current state is 0.
      extract: _ => 0f,
      defaultValue: Defaults.float_
    ) {}

    public override void invalidate() {
      base.invalidate();
      pathBehaviour.invalidate();
      _mutator = null;
    }
    
    // While constructor is running field values are still defaults, because Unity has not deserialized them yet.
    // 
    // If we try to access serialized fields in the constructor, we get a null reference exception,
    // therefore we need to calculate the mutator on demand, after Unity has deserialized field values.
    TweenMutator<float, Transform> _mutator;
    protected override TweenMutator<float, Transform> mutator => 
      _mutator ?? (_mutator = TweenMutators.path(pathBehaviour.path));
    
    protected override float convert(float p) => p;
  }
}