using AdvancedInspector;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  public abstract class ComplexSerializedEase : ComponentMonoBehaviour {
    public abstract string easeName { get; }
    public abstract Ease ease { get; }

    public override string ToString() => easeName;
  }
}