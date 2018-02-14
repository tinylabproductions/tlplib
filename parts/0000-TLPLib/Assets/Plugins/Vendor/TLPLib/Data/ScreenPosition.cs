using System;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  /**
   * Position in screen space. [(0, 0), (Screen.width, Screen.height)]
   **/
  [Record]
  public partial struct ScreenPosition {
    public readonly Vector2 position;

    public static ScreenPosition operator +(ScreenPosition sp, Vector2 v) => new ScreenPosition(sp.position + v);
  }
}