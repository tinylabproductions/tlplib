using System;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.functional;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public delegate ImmutableList<ErrorMsg> SceneValidator(Scene scene);

  public static class SceneValidator_ {
    public static readonly SceneValidator
      validateForOneRootObject = validateForNRootObjects(1),
      validateForNoRootObjects = validateForNRootObjects(0),
      noValidations = scene => ImmutableList<ErrorMsg>.Empty;

    public static SceneValidator validateForComponent<A>() where A : Component =>
      scene => {
        var aList = scene.GetRootGameObjects().collect(go => go.GetComponentSafe<A>()).ToImmutableList();
        return (aList.Count != 1).opt(new ErrorMsg(
          $"Found {aList.Count} of {typeof(A)} in scene '{scene.path}' root game objects, expected 1."
        )).asEnumerable.ToImmutableList();
      };

    public static SceneValidator validateForNRootObjects(int n) =>
      scene => {
        var rootObjectCount = scene.GetRootGameObjects().Length;
        return (rootObjectCount != n).opt(
          new ErrorMsg($"Expected {n} root game objects but found {rootObjectCount}")
        ).asEnumerable.ToImmutableList();
      };

    public static SceneValidator validateForGameObjectWithComponent<C>(string path) where C : Component =>
      scene => (
        from go in GameObject.Find(path).opt().toRight(new ErrorMsg($"Can't find GO at path {path}"))
        from _ in go.GetComponentSafeE<C>()
        select _
      ).leftValue.asEnumerable.ToImmutableList();
  }

  public static class WithSceneValidator {
    public static WithSceneValidator<A> a<A>(A a, SceneValidator validator) =>
      new WithSceneValidator<A>(a, validator);
  }

  public struct WithSceneValidator<A> {
    public readonly A a;
    public readonly SceneValidator validator;

    public WithSceneValidator(A a, SceneValidator validator) {
      this.a = a;
      this.validator = validator;
    }

    public WithSceneValidator<B> map<B>(Func<A, B> mapper) =>
      new WithSceneValidator<B>(mapper(a), validator);
  }

  public static class SceneValidatorExts {
    public static SceneValidator join(
      this SceneValidator a, SceneValidator b
    ) => scene => a(scene).AddRange(b(scene));

    public static SceneValidator createComponentValidator<A>(
      this RuntimeSceneRefWithComponent<A> sceneRef
    ) where A : Component => SceneValidator_.validateForComponent<A>();

    public static WithSceneValidator<ScenePath> toSceneNameAndValidator<A>(
      this RuntimeSceneRefWithComponent<A> sceneRef
    ) where A : Component => WithSceneValidator.a(sceneRef.scenePath, sceneRef.createComponentValidator());
  }
}