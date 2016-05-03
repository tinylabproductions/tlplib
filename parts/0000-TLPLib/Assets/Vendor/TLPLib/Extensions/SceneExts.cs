using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SceneExts {
    // includes inactive objects
    // may not work on scene awake
    // http://forum.unity3d.com/threads/bug-getrootgameobjects-is-not-working-in-awake.379317/
    public static IEnumerable<T> findObjectsOfTypeAll<T>(this Scene scene) where T : UnityEngine.Object {
      return scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<T>(true));
    }
  }
}
