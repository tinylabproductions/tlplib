using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.InputUtils {
  public static class InputExts {
    // ReSharper disable ConvertClosureToMethodGroup
    [PublicAPI] public static bool getKey(this IList<KeyCode> kcs) => kcs.anyGCFree(_ => Input.GetKey(_));
    [PublicAPI] public static bool getKeyDown(this IList<KeyCode> kcs) => kcs.anyGCFree(_ => Input.GetKeyDown(_));
    [PublicAPI] public static bool getKeyUp(this IList<KeyCode> kcs) => kcs.anyGCFree(_ => Input.GetKeyUp(_));
    // ReSharper restore ConvertClosureToMethodGroup
  }
}