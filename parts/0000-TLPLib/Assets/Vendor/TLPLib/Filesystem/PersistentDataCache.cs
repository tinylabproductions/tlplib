using System.IO;
using System.Text;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Filesystem {
  class PersistentDataCache {
    static readonly Encoding defaultEncoding = Encoding.UTF8;
    static readonly PathStr root = new PathStr(Application.persistentDataPath);

    public static PathStr fullPath(string name) { return root / name; }

    public static Option<Try<byte[]>> read(PathStr path) {
      return File.Exists(path)
        ? F.doTry(() => File.ReadAllBytes(path)).some()
        : F.none<Try<byte[]>>();
    }

    public static Option<Try<string>> readString(PathStr path, Encoding encoding=null) {
      encoding = encoding ?? defaultEncoding;
      return read(path).map(t => t.map(bytes => encoding.GetString(bytes)));
    }

    public static Try<Unit> store(PathStr path, byte[] data) {
      return F.doTry(() => File.WriteAllBytes(path, data));
    }

    public static Try<Unit> storeString(PathStr path, string data, Encoding encoding = null) {
      encoding = encoding ?? defaultEncoding;
      return store(path, encoding.GetBytes(data));
    }
  }
}
