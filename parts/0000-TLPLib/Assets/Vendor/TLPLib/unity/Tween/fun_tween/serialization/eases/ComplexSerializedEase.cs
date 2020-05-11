
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases {
  public abstract class ComplexSerializedEase : MonoBehaviour, Invalidatable {
    public abstract string easeName { get; }
    protected abstract Ease createEase();

    Ease _ease;
    public Ease ease => _ease ??= createEase();

    public void invalidate() => _ease = null;
    public override string ToString() => easeName;
  }
}