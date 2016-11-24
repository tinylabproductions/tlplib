using System.IO;
using System.Linq;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public static class PathStrExts {
    public static PathStr toAbsoluteRelativeToProjectDir(this PathStr path) {
      var full = path.path.Contains(':') || path.path.StartsWith("/")
        ? path
        : Application.dataPath + "/../" + path;

      return new PathStr(Path.GetFullPath(full));
    }
  }
}