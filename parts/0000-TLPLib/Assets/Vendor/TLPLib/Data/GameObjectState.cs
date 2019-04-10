using System;
using System.Collections.Generic;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable, PublicAPI] public partial class GameObjectState {
#pragma warning disable 649
    [SerializeField, NotNull, PublicAccessor] GameObject _gameObject;
    [SerializeField, NotNull, PublicAccessor] bool _active;
#pragma warning restore 649

    public void apply() => _gameObject.SetActive(_active);
    public void invertedApply() => _gameObject.SetActive(!_active);
  }

  [PublicAPI] public static class GameObjectStateExts {
    public static void apply(this IList<GameObjectState> states) {
      foreach (var state in states) {
        state.apply();
      }
    } 
    
    public static void invertedApply(this IList<GameObjectState> states) {
      foreach (var state in states) {
        state.invertedApply();
      }
    }

    public static void apply(this IList<GameObjectState> states, bool normal) {
      if (normal) states.apply();
      else states.invertedApply();
    }
  }
}