using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.InputUtils {
  public static class InputExts {
    [PublicAPI] public static bool getKey(this IList<KeyCode> kcs) => kcs.anyGCFree(Input.GetKey); 
    [PublicAPI] public static bool getKeyDown(this IList<KeyCode> kcs) => kcs.anyGCFree(Input.GetKeyDown); 
    [PublicAPI] public static bool getKeyUp(this IList<KeyCode> kcs) => kcs.anyGCFree(Input.GetKeyUp); 
  }
}