using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public delegate ImmutableList<ErrorMsg> SceneValidator(Scene scene);

  public struct SceneWithValidator {
    public readonly ScenePath path;
    public readonly SceneValidator validator;

    public SceneWithValidator(ScenePath path, SceneValidator validator) {
      this.path = path;
      this.validator = validator;
    }

    public static SceneWithValidator a(ScenePath path, SceneValidator validator) =>
      new SceneWithValidator(path, validator);
  }

  public static class SceneValidatorExts {
    public static SceneWithValidator withValidator(this ScenePath path, SceneValidator validator) =>
      SceneWithValidator.a(path, validator);

    public static SceneValidator join(
      this SceneValidator a, SceneValidator b
    ) => scene => a(scene).AddRange(b(scene));

    public static SceneValidator validateForComponent<A>(
      this RuntimeSceneRefWithComponent<A> sceneRef
    ) where A : Component => validateForComponent<A>();

    public static Tpl<SceneName, SceneValidator> toSceneNameAndValidator<A>(
      this RuntimeSceneRefWithComponent<A> sceneRef
    ) where A : Component => F.t(sceneRef.sceneName, sceneRef.validateForComponent());

    public static SceneValidator validateForComponent<A>() where A : Component =>
      scene => {
        var ass = scene.GetRootGameObjects().collect(go => go.GetComponent<A>().opt()).ToImmutableList();
        return (ass.Count != 1).opt(new ErrorMsg(
          $"Found {ass.Count} of {typeof(A)} in scene '{scene.path}' root game objects, expected 1."
        )).toImmutableList();
      };

    public static SceneValidator validateForOneRootObject = validateForNRootObjects(1);
    public static SceneValidator validateForNoRootObjects = validateForNRootObjects(0);

    public static SceneValidator validateForNRootObjects(int n) =>
      scene => {
        var rootObjectCount = scene.GetRootGameObjects().Length;
        return (rootObjectCount != n).opt(
          new ErrorMsg($"Expected {n} root game objects but found {rootObjectCount}")
        ).toImmutableList();
      };

    public static SceneValidator validateForGameObjectWithComponent<C>(string path) where C : Component =>
      scene => GameObject.Find(path).opt()
        .toRight(new ErrorMsg($"Can't find GO at path {path}"))
        .flatMapRight(_ => _.GetComponent<C>().opt().toRight(new ErrorMsg($"Can't find component {typeof(C)}")))
        .leftValue
        .toImmutableList();

    public static SceneValidator validateEmpty = scene => ImmutableList<ErrorMsg>.Empty;
  }
}