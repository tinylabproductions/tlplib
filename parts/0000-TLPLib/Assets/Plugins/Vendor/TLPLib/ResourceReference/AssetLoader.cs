using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.ResourceReference {
  public class AssetLoader<A> where A : Object {
    readonly string assetName, resourcesSubDir;
    readonly PathStr generatedDir;

    public AssetLoader(string assetName, string resourcesSubDir, PathStr generatedDir) {
      this.assetName = assetName;
      this.resourcesSubDir = resourcesSubDir;
      this.generatedDir = generatedDir;
    }

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
}