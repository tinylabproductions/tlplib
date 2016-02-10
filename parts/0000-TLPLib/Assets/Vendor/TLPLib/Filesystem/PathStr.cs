using System.IO;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public struct PathStr {
    public readonly string path;

    public PathStr(string path) { this.path = path; }

    public static PathStr operator /(PathStr s1, string s2) {
      return new PathStr(Path.Combine(s1.path, s2));
    }

    public static implicit operator string(PathStr s) { return s.path; }

    public PathStr dirname => new PathStr(Path.GetDirectoryName(path));

    public override string ToString() { return path; }
    public string unixString => ToString().Replace(@"\", "/");
  }
}
