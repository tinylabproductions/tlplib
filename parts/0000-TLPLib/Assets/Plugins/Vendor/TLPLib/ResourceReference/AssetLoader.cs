using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public interface IAssetLoader<A> {
    A loadAsset();
    Tpl<ResourceRequest, Future<A>> loadAssetAsync();
  }

  public static class AssetLoaderExts {
    public static IAssetLoader<B> map<A, B>(this IAssetLoader<A> loader, Fn<A, B> mapper) =>
      new AssetLoaderMapped<A, B>(loader, mapper);
  }

  [Record]
  public partial class AssetLoader<A> : IAssetLoader<A> where A : Object {
    public readonly string assetName, resourcesSubDir;
    public readonly PathStr generatedDir;

    string prefabName => $"{assetName}.prefab";
    public PathStr assetEditorResourcePath => generatedDir / prefabName;
    public PathStr assetRuntimeResourcePath => PathStr.a(resourcesSubDir) / assetName;

    public A loadAsset() => ResourceReference.load<A>(assetRuntimeResourcePath).rightOrThrow;

    public Tpl<ResourceRequest, Future<A>> loadAssetAsync() {
      var loaded = ResourceReference.loadAsync<A>(assetRuntimeResourcePath);
      var valuedFuture = loaded._2.flatMap(either => {
        if (either.isRight) return Future<A>.successful(either.rightOrThrow);
        Log.d.error(either.leftOrThrow);
        return Future<A>.unfulfilled;
      });
      return F.t(loaded._1, valuedFuture);
    }
  }

  [Record]
  public partial class AssetLoaderMapped<A, B> : IAssetLoader<B> {
    readonly IAssetLoader<A> loader;
    readonly Fn<A, B> mapper;

    public B loadAsset() => mapper(loader.loadAsset());
    public Tpl<ResourceRequest, Future<B>> loadAssetAsync() => loader.loadAssetAsync().map2(_ => _.map(mapper));
  }
}