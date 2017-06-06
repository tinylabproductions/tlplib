using System.IO;
using com.tinylabproductions.TLPLib.Filesystem;

namespace Plugins.Vendor.TLPLib.Extensions {
  public static class DirectoryExts {
    public static void completeDelete(PathStr path) {
      var files = Directory.GetFiles(path);
      var dirs = Directory.GetDirectories(path);

      foreach (var file in files) {
        File.SetAttributes(file, FileAttributes.Normal);
        File.Delete(file);
      }

      foreach (var dir in dirs) completeDelete(PathStr.a(dir));
      
      Directory.Delete(path, false);
    }
  }
}