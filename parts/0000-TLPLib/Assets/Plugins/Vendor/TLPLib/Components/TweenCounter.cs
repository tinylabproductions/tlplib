using System;
using com.tinylabproductions.TLPLib.Tween.fun_tween;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Components {
  public class TweenCounter : InfoWindow {
    public const int LIMIT = 50;

    protected override Rect initialRect() => new Rect(10, 10, 250, 50);
    protected override bool dragAllowed => true;

    protected override Fn<Color> updateColorFn => () => {
      var count = TweenManagerRunner.instance.currentlyRunningTweenCount;
      if (count <= LIMIT / 2) return Color.white;
      else if (count <= LIMIT) return Color.yellow;
      return Color.red;
    };

    protected override Fn<string> text => () =>
      $"Currently running tween count : {TweenManagerRunner.instance.currentlyRunningTweenCount}";

    public static TweenCounter create() {
      var go = new GameObject(nameof(TweenCounter));
      DontDestroyOnLoad(go);
      return go.AddComponent<TweenCounter>();
    }
  }
}