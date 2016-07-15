using System.IO;
using System.Text;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Filesystem {
  class PersistentDataCache {
    static readonly Encoding defaultEncoding = Encoding.UTF8;
    static readonly Option<PathStr> root = Application.persistentDataPath.nonEmptyOpt(trim: true).map(PathStr.a);

    public static Option<PathStr> fullPath(string name) {
      foreach (var r in root) return F.some(r / name);
      return F.none<PathStr>();
    }

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
