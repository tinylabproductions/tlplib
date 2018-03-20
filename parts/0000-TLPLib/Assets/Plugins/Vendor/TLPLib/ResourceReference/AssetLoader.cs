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
    
    string assetName { get; }
    string assetRuntimeResourceDirectory { get; }
    PathStr assetRuntimeResourcePath { get; }
    PathStr assetEditorResourceDirectory { get; }
    PathStr assetEditorResourcePath { get; }
  }

  public static class AssetLoaderExts {
    public static IAssetLoader<B> map<A, B>(this IAssetLoader<A> loader, Fn<A, B> mapper) =>
      new AssetLoaderMapped<A, B>(loader, mapper);
  }

  public class AssetLoader<A> : IAssetLoader<A> where A : Object {
    public string assetName { get; }
    public string assetRuntimeResourceDirectory { get; }
    public PathStr assetEditorResourceDirectory { get; }

    public AssetLoader(string assetName, string assetRuntimeResourceDirectory, PathStr assetEditorResourceDirectory) {
      this.assetName = assetName;
      this.assetRuntimeResourceDirectory = assetRuntimeResourceDirectory;
      this.assetEditorResourceDirectory = assetEditorResourceDirectory;
    }

    public override string ToString() => $"{nameof(AssetLoader<A>)}({typeof(A)} @ {assetEditorResourcePath})";
    
    public PathStr assetEditorResourcePath => assetEditorResourceDirectory / $"{assetName}.asset";
    public PathStr assetRuntimeResourcePath => PathStr.a(assetRuntimeResourceDirectory) / assetName;

    public A loadAsset() => ResourceLoader.load<A>(assetRuntimeResourcePath).rightOrThrow;

    public Tpl<ResourceRequest, Future<A>> loadAssetAsync() {
      var loaded = ResourceLoader.loadAsync<A>(assetRuntimeResourcePath);
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
    public string assetName => loader.assetName;
    public string assetRuntimeResourceDirectory => loader.assetRuntimeResourceDirectory;
    public PathStr assetRuntimeResourcePath => loader.assetRuntimeResourcePath;
    public PathStr assetEditorResourceDirectory => loader.assetEditorResourceDirectory;
    public PathStr assetEditorResourcePath => loader.assetEditorResourcePath;
  }
}