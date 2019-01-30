using System;
using System.Collections.Generic;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  [Serializable] public partial class GameObjectState {
#pragma warning disable 649
    [SerializeField, NotNull, PublicAccessor] GameObject _gameObject;
    [SerializeField, NotNull, PublicAccessor] bool _active;
#pragma warning restore 649

    [PublicAPI] public void apply() => _gameObject.SetActive(_active);
    [PublicAPI] public void invertedApply() => _gameObject.SetActive(!_active);
  }

  public static class GameObjectStateExts {
    [PublicAPI] public static void apply(this IList<GameObjectState> states) {
      foreach (var state in states) {
        state.apply();
      }
    } 
    
    [PublicAPI] public static void invertedApply(this IList<GameObjectState> states) {
      foreach (var state in states) {
        state.invertedApply();
      }
    } 
  }
}