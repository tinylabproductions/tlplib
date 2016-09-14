using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public class PersistentDataCache : ICache<byte[]> {
    public static Option<ICache<byte[]>> instance = 
      Application.persistentDataPath.nonEmptyOpt(trim: true).map<ICache<byte[]>>(path => 
        new PersistentDataCache(new PathStr(path))
      );

    public static Option<ICache<string>> stringInstance =
      instance.map(c => c.bimap(BiMapper.utf8ByteArrString));

    readonly PathStr root;

    PersistentDataCache(PathStr root) { this.root = root; }

    public ICachedBlob<byte[]> blobFor(string name) => new FileCachedBlob(root / name);
  }
}
