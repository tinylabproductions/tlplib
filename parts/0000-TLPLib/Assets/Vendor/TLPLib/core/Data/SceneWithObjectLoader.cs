using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.concurrent;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Data {
  public static class SceneWithObjectLoader {
    public static Future<Either<ErrorMsg, A>> load<A>(
      ScenePath scenePath, LoadSceneMode loadSceneMode = LoadSceneMode.Single
    ) where A : Component =>
      Future.successful(
        F.doTry(() => SceneManager.LoadSceneAsync(scenePath, loadSceneMode))
          .toEither().mapLeft(err => new ErrorMsg($"Error while loading scene '{scenePath}': {err}"))
      ).flatMapT(op => op.toFuture().map(_ =>
        SceneManager.GetSceneByPath(scenePath).findComponentOnRootGameObjects<A>()
      ));
  }
}