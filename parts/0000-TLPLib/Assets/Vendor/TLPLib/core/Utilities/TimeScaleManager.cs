using pzd.lib.collection;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  public class TimeScaleMultiplier {
    bool _active;
    float _multiplier;

    static TimeScaleMultiplier[] list = EmptyArray<TimeScaleMultiplier>._;
    static uint listCount;

    static void refresh() {
      var newScale = 1f;
      foreach (var tsm in list) newScale *= tsm._multiplier;
      Time.timeScale = newScale;
    }

    public static TimeScaleMultiplier a(float multiplier, bool active) {
      var tsm = new TimeScaleMultiplier(multiplier);
      tsm.active = active;
      return tsm;
    }

    TimeScaleMultiplier(float multiplier) {
      _multiplier = multiplier;
    }

    public bool active {
      get => _active;
      set {
        if (_active) {
          if (!value) {
            AList.removeReplacingWithLast(list, ref listCount, this);
            refresh();
          }
        }
        else {
          if (value) {
            AList.add(ref list, ref listCount, this);
            refresh();
          }
        }
        _active = value;
      }
    }

    public float multiplier {
      get => _multiplier;
      set {
        _multiplier = value;
        if (_active) refresh();
      }
    }
  }
}