using System.IO;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public static class PathStrExts {
    public static PathStr toAbsoluteRelativeToProjectDir(this PathStr path) {
      var full = path.path.Contains(':') || path.path.StartsWithFast("/")
        ? path
        : Application.dataPath + "/../" + path;

      return new PathStr(Path.GetFullPath(full));
    }
  }
}