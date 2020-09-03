using System;
using System.Collections.Generic;
using pzd.lib.reactive;

using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Utilities {
  public class ToggleableMultiplier : IDisposable {
    public class Manager {
      readonly List<ToggleableMultiplier> list = new List<ToggleableMultiplier>();
      readonly IRxRef<float> _totalMultiplier = RxRef.a(1f);
      public IRxVal<float> totalMultiplier => _totalMultiplier;

      /// <summary>
      /// Creates toggleable multipliers that are connected with the value of totalMultiplier
      /// totalMultiplier.value == product of all active multipliers
      /// </summary>
      public ToggleableMultiplier createMultiplier(float multiplier, bool active = true) =>
        new ToggleableMultiplier(multiplier, list, _totalMultiplier, active);
    }

    readonly List<ToggleableMultiplier> list;
    readonly IRxRef<float> totalMultiplier;
    bool _active;
    float _multiplier;

    ToggleableMultiplier(
      float multiplier, List<ToggleableMultiplier> list,
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
            list.removeReplacingWithLast(this);
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
