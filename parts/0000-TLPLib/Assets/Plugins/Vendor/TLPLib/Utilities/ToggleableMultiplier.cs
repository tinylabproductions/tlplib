using System;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Utilities {
  public class ToggleableMultiplier : IDisposable {
    public class Manager {
      readonly RandomList<ToggleableMultiplier> list = new RandomList<ToggleableMultiplier>();
      readonly IRxRef<float> _totalMultiplier = RxRef.a(1f);
      public IRxVal<float> totalMultiplier => _totalMultiplier;

      /// <summary>
      /// Collects all created multipliers, and calculates their total multiplier
      /// </summary>
      public ToggleableMultiplier createMultiplier(float multiplier, bool active = true) =>
        new ToggleableMultiplier(multiplier, list, _totalMultiplier, active);
    }

    readonly RandomList<ToggleableMultiplier> list;
    readonly IRxRef<float> totalMultiplier;
    bool _active;
    float _multiplier;

    ToggleableMultiplier(
      float multiplier, RandomList<ToggleableMultiplier> list, 
      IRxRef<float> totalMultiplier, bool active
    ) {
      _multiplier = multiplier;
      this.list = list;
      this.totalMultiplier = totalMultiplier;
      _active = active;
    }

    void refresh() {
      var newScale = 1f;
      foreach (var mult in list) newScale *= mult._multiplier;
      totalMultiplier.value = newScale;
    }

    public bool active {
      get { return _active; }
      set {
        if (_active) {
          if (!value) {
            list.Remove(this);
            refresh();
          }
        }
        else {
          if (value) {
            list.Add(this);
            refresh();
          }
        }
        _active = value;
      }
    }

    public float multiplier {
      get { return _multiplier; }
      set {
        _multiplier = value;
        if (_active) refresh();
      }
    }

    public void Dispose() {
      active = false;
    }
  }
}
