using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public interface ILoader<A> {
    A loadSync();
    Tpl<IAsyncOperation, Future<A>> loadASync();
  }
  
  [Record]
  public partial class LoaderMapped<A, B> : ILoader<B> {
    readonly ILoader<A> loader;
    readonly Func<A, B> mapper;

    public B loadSync() => mapper(loader.loadSync());
    public Tpl<IAsyncOperation, Future<B>> loadASync() => 
      loader.loadASync().map2(_ => _.map(mapper));
  }

  public static class LoaderExts {
    [PublicAPI] public static ILoader<B> map<A, B>(this ILoader<A> loader, Func<A, B> mapper) =>
      new LoaderMapped<A, B>(loader, mapper);
  }
  
  public interface IAssetLoader {
    string assetName { get; }
    string assetRuntimeResourceDirectory { get; }
    PathStr assetRuntimeResourcePath { get; }
    PathStr assetEditorResourceDirectory { get; }
    PathStr assetEditorResourcePath { get; }
  }
  
  public interface IAssetLoader<A> : IAssetLoader, ILoader<A> {}

  public static class AssetLoaderExts {
    [PublicAPI] public static IAssetLoader<B> map<A, B>(this IAssetLoader<A> loader, Func<A, B> mapper) =>
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

    public A loadSync() => ResourceLoader.load<A>(assetRuntimeResourcePath).rightOrThrow;

    public Tpl<IAsyncOperation, Future<A>> loadASync() {
      var loaded = ResourceLoader.loadAsync<A>(assetRuntimeResourcePath);
      var valuedFuture = loaded._2.flatMap(either => {
        if (either.isRight) return Future<A>.successful(either.rightOrThrow);
        Log.d.error(either.leftOrThrow);
        return Future<A>.unfulfilled;
      });
      return F.t(loaded._1.upcast(default(IAsyncOperation)), valuedFuture);
    }
  }

  public partial class AssetLoaderMapped<A, B> : LoaderMapped<A, B>, IAssetLoader<B> {
    readonly IAssetLoader<A> loader;

    public AssetLoaderMapped(IAssetLoader<A> loader, Func<A, B> mapper) : base(loader, mapper) {
      this.loader = loader;
    }

    public string assetName => loader.assetName;
    public string assetRuntimeResourceDirectory => loader.assetRuntimeResourceDirectory;
    public PathStr assetRuntimeResourcePath => loader.assetRuntimeResourcePath;
    public PathStr assetEditorResourceDirectory => loader.assetEditorResourceDirectory;
    public PathStr assetEditorResourcePath => loader.assetEditorResourcePath;
  }
}