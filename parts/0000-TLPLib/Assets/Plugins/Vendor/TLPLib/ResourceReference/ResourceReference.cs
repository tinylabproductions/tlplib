using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  /// <summary>
  /// A thing you save in to resources folder that allows you to reference something outside
  /// of resources folder.
  /// </summary>
  public abstract partial class ResourceReference<A> : ScriptableObject where A : Object {
#pragma warning disable 649
    [SerializeField, NotNull, PublicAccessor] A _reference;
#pragma warning restore 649

#if UNITY_EDITOR
    public A editorReference {
      set => _reference = value;
    }
#endif
  }

  public static class ResourceReference {
#if UNITY_EDITOR
    [PublicAPI]
    public static SO create<SO, A>(string path, A reference)
      where SO : ResourceReference<A> where A : Object
    {
      var so = ScriptableObject.CreateInstance<SO>();
      so.editorReference = reference;
      UnityEditor.AssetDatabase.CreateAsset(so, path);
      return so;
    }
#endif

    [PublicAPI]
    public static Either<ErrorMsg, A> load<A>(PathStr loadPath) where A : Object => 
      ResourceLoader.load<ResourceReference<A>>(loadPath).mapRight(_ => _.reference);

    [PublicAPI]
    public static Tpl<IAsyncOperation, Future<Either<ErrorMsg, A>>> loadAsync<A>(
      PathStr loadPath
    ) where A : Object =>
      ResourceLoader.loadAsync<ResourceReference<A>>(loadPath)
        .map2(future => future.mapT(resourceReference => {
          var reference = resourceReference.reference;
          Resources.UnloadAsset(resourceReference);
          return reference;
      }));

    [PublicAPI]
    public static Tpl<IAsyncOperation, Future<A>> loadAsyncIgnoreErrors<A>(
      PathStr loadPath, bool logOnError = true
    ) where A : Object =>
      ResourceLoader.loadAsyncIgnoreErrors<ResourceReference<A>>(loadPath, logOnError)
        .map2(future => future.map(_ => _.reference));
  }
}
