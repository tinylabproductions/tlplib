using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class SceneExts {
    // includes inactive objects
    // may not work on scene awake
    // http://forum.unity3d.com/threads/bug-getrootgameobjects-is-not-working-in-awake.379317/
    public static IEnumerable<T> findObjectsOfTypeAll<T>(this Scene scene) where T : Object {
      return scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<T>(true));
    }

    /// <summary>
    /// Retrieve first <see cref="A"/> attached to a root <see cref="GameObject"/> in the <see cref="Scene"/>.
    /// </summary>
    public static Either<ErrorMsg, A> findComponentOnRootGameObjects<A>(this Scene scene) where A : Component => 
      scene.GetRootGameObjects()
      .collectFirst(go => go.GetComponent<A>().opt())
      .fold(
        () => F.left<ErrorMsg, A>(new ErrorMsg(
          $"Couldn't find {typeof(A)} in scene '{scene.path}' root game objects"
        )),
        F.right<ErrorMsg, A>
      );
  }
}
