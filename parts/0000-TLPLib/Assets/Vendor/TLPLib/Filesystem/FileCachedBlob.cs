using System.IO;
using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public class FileCachedBlob : ICachedBlob<byte[]> {
    public readonly PathStr path;

    public FileCachedBlob(PathStr path) { this.path = path; }

    public override string ToString() => $"{nameof(FileCachedBlob)}[{path}]";

    public bool cached => File.Exists(path);

    public Option<Try<byte[]>> read() => 
      File.Exists(path)
      ? F.doTry(() => File.ReadAllBytes(path)).some()
      : F.none<Try<byte[]>>();

    public Try<Unit> store(byte[] data) => 
      F.doTry(() => File.WriteAllBytes(path, data));

    public Try<Unit> clear() => F.doTry(() => File.Delete(path));
  }
}